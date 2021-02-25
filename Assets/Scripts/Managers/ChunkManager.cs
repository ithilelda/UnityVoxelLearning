using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public class ChunkManager : MonoBehaviour
{
    public FastNoiseLite Noise { get; } = new FastNoiseLite();
    public Dictionary<ChunkId, ChunkData> ChunkDatas { get; } = new Dictionary<ChunkId, ChunkData>();
    public Dictionary<ChunkId, ChunkView> ChunkViews { get; } = new Dictionary<ChunkId, ChunkView>();

    [Range(0,2)]
    public int MeshingType;
    public bool fullUpdate;
    public float frequency;

    private readonly Queue<BatchMeshingJob> meshingJobs = new Queue<BatchMeshingJob>();
    private readonly Queue<BatchBakingJob> bakingJobs = new Queue<BatchBakingJob>();
    private readonly Queue<ChunkId> dirtyChunks = new Queue<ChunkId>();

    public static ChunkId FromWorldPos(int x, int y, int z)
    {
        return new ChunkId(x >> GameDefines.CHUNK_BIT, y >> GameDefines.CHUNK_BIT, z >> GameDefines.CHUNK_BIT);
    }
    public static ChunkId FromWorldPos(Vector3Int index)
    {
        return FromWorldPos(index.x, index.y, index.z);
    }

    public uint this[int x, int y, int z]
    {
        get
        {
            var chunk = ChunkDatas[FromWorldPos(x, y, z)];
            return chunk[x & GameDefines.CHUNK_MASK, y & GameDefines.CHUNK_MASK, z & GameDefines.CHUNK_MASK];
        }

        set
        {
            var chunk = ChunkDatas[FromWorldPos(x, y, z)];
            chunk[x & GameDefines.CHUNK_MASK, y & GameDefines.CHUNK_MASK, z & GameDefines.CHUNK_MASK] = value;
            chunk.IsDirty = true;
        }
    }
    public uint this[Vector3Int index]
    {
        get
        {
            var chunk = ChunkDatas[FromWorldPos(index)];
            return chunk[index.x & GameDefines.CHUNK_MASK, index.y & GameDefines.CHUNK_MASK, index.z & GameDefines.CHUNK_MASK];
        }

        set
        {
            var chunk = ChunkDatas[FromWorldPos(index)];
            chunk[index.x & GameDefines.CHUNK_MASK, index.y & GameDefines.CHUNK_MASK, index.z & GameDefines.CHUNK_MASK] = value;
            chunk.IsDirty = true;
        }
    }

    public Vector3Int GetCoordinateFromHit(Vector3 hitPos, Vector3 faceNormal)
    {
        var loc = hitPos - Vector3.Max(Vector3.zero, faceNormal);
        return Vector3Int.FloorToInt(loc);
    }

    public void GenerateChunk(ChunkId id)
    {
        var go = new GameObject($"Chunk {id.x} {id.y} {id.z}")
        {
            layer = 6 // put all chunks on the layer 6.
        };
        go.transform.parent = transform.parent;
        var startPos = id.WorldPosition();
        go.transform.Translate(startPos); // moves the chunk to the correct position.
        var chunkData = go.AddComponent<ChunkData>();
        chunkData.ChunkId = id;
        chunkData.ChunkSystem = this;
        for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    //Debug.Log(pos);
                    chunkData[x, y, z] = (uint)(Mathf.FloorToInt(Noise.GetNoise(startPos.x + x, startPos.y + y, startPos.z + z)) + 1);
                }
            }
        }
        chunkData.IsDirty = true;
        ChunkDatas.Add(id, chunkData);
        var chunkView = go.AddComponent<ChunkView>();
        ChunkViews.Add(id, chunkView);
    }

    private void Start()
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(frequency);
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    var id = new ChunkId(x, y, z);
                    GenerateChunk(id);
                }
            }
        }
        ComputeShader shader = Resources.Load<ComputeShader>("ComputeShaders/MeshGeneration");
        Debug.Log(shader.ToString());
    }

    private void Update()
    {
        if (fullUpdate)
        {
            foreach (var p in ChunkDatas)
            {
                p.Value.IsDirty = true;
            }
        }
        var start = Time.realtimeSinceStartup;
        DoMeshGeneration();
        var end = Time.realtimeSinceStartup;
        if (fullUpdate)
        {
            Debug.Log($"setup jobs: {end - start}");
        }
    }
    private void LateUpdate()
    {
        HandleJobCompletion();
        if (fullUpdate) fullUpdate = false;
    }

    private void DoMeshGeneration()
    {
        foreach (var p in ChunkDatas)
        {
            if (p.Value.IsDirty)
            {
                dirtyChunks.Enqueue(p.Key);
                p.Value.IsDirty = false;
            }
        }
        if (dirtyChunks.Count > 0)
        {
            switch (MeshingType)
            {
                case 0:
                    DoSyncMeshing(dirtyChunks);
                    break;
                case 1:
                    ScheduleMeshJob(dirtyChunks);
                    break;
                case 2:
                    ScheduleGreedyMeshingJob(dirtyChunks);
                    break;
            }
        }
    }
    private void HandleJobCompletion()
    {
        if (meshingJobs.Count > 0)
        {
            var mj = meshingJobs.Dequeue();
            mj.HandleJobCompletion();
            if (mj.IsCompleted)
            {
                var bakingJob = new BatchBakingJob(mj.Meshes);
                bakingJobs.Enqueue(bakingJob);
            }
            else
            {
                meshingJobs.Enqueue(mj);
            }
        }
        if (bakingJobs.Count > 0)
        {
            var bj = bakingJobs.Dequeue();
            if (bj.Handle.IsCompleted)
            {
                bj.Handle.Complete();
                bj.Ids.Dispose();
                foreach (var p in ChunkViews)
                {
                    p.Value.SetBakedMesh();
                }
            }
            else
            {
                bakingJobs.Enqueue(bj);
            }
        }
    }

    private void DoSyncMeshing(Queue<ChunkId> ids)
    {
        while (ids.Count > 0)
        {
            var id = ids.Dequeue();
            ChunkViews[id].RenderToMesh(id, ChunkDatas[id]);
        }
    }
    private void ScheduleMeshJob(Queue<ChunkId> ids)
    {
        var dataArray = Mesh.AllocateWritableMeshData(ids.Count);
        var batchJob = new BatchMeshingJob(ids.Count)
        {
            Array = dataArray,
            Manager = this
        };
        var index = 0;
        while (ids.Count > 0)
        {
            var id = ids.Dequeue();
            batchJob.Meshes[index] = ChunkViews[id].GetMesh();
            var mesh = new NativeMeshData
            {
                Vertices = new NativeArray<VertexData>(GameDefines.MAXIMUM_VERTEX_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Triangles = new NativeArray<uint>(GameDefines.MAXIMUM_TRIANGLE_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Indices = new NativeArray<int>(2, Allocator.TempJob)
            };
            var data = new NativeArray<uint>(GameDefines.CHUNK_SIZE_CUBED, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            data.CopyFrom(ChunkDatas[id].Voxels);
            var job = new JobNaiveCulling
            {
                Data = data,
                MeshData = mesh
            };
            var apply = new JobApplyMesh
            {
                Data = mesh,
                MeshData = dataArray[index]
            };
            batchJob.Jobs.Enqueue(new MeshingJob { Id = id, Job = job, Handle = apply.Schedule(job.Schedule()) });
            index++;
        }
        meshingJobs.Enqueue(batchJob);
    }
    private void ScheduleGreedyMeshingJob(Queue<ChunkId> ids)
    {
        var dataArray = Mesh.AllocateWritableMeshData(ids.Count);
        var batchJob = new BatchMeshingJob(ids.Count)
        {
            Array = dataArray,
            Manager = this
        };
        var index = 0;
        while (ids.Count > 0)
        {
            var id = ids.Dequeue();
            batchJob.Meshes[index] = ChunkViews[id].GetMesh();
            var mesh = new NativeMeshData
            {
                Vertices = new NativeArray<VertexData>(GameDefines.MAXIMUM_VERTEX_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Triangles = new NativeArray<uint>(GameDefines.MAXIMUM_TRIANGLE_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Indices = new NativeArray<int>(2, Allocator.TempJob)
            };
            var data = new NativeArray<uint>(GameDefines.CHUNK_SIZE_CUBED, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            data.CopyFrom(ChunkDatas[id].Voxels);
            var job = new JobGreedyMeshing
            {
                Data = data,
                MeshData = mesh
            };
            var apply = new JobApplyMesh
            {
                Data = mesh,
                MeshData = dataArray[index]
            };
            batchJob.Jobs.Enqueue(new MeshingJob { Id = id, Job = job, Handle = apply.Schedule(job.Schedule()) });
            index++;
        }
        meshingJobs.Enqueue(batchJob);
    }
}