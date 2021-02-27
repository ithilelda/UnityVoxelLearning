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

    private readonly Queue<ChunkId> firstDirtyChunks = new Queue<ChunkId>();
    private readonly Queue<ChunkId> secondDirtyChunks = new Queue<ChunkId>();
    private bool dirtyQueueFirst;

    private BatchMeshingJob meshingJob;
    private readonly Queue<BatchBakingJob> bakingJobs = new Queue<BatchBakingJob>();

    public static ChunkId FromWorldPos(int x, int y, int z)
    {
        return new ChunkId(x >> GameDefines.CHUNK_BIT, y >> GameDefines.CHUNK_BIT, z >> GameDefines.CHUNK_BIT);
    }
    public static ChunkId FromWorldPos(Vector3Int index)
    {
        return FromWorldPos(index.x, index.y, index.z);
    }

    public Queue<ChunkId> CurrentDirtyQueue
    {
        get => dirtyQueueFirst ? firstDirtyChunks : secondDirtyChunks;
    }
    public bool EnqueueDirtyChunk(ChunkId id)
    {
        Queue<ChunkId> dc = dirtyQueueFirst ? secondDirtyChunks : firstDirtyChunks;
        if (dc.Contains(id))
        {
            return false;
        }
        else
        {
            dc.Enqueue(id);
            return true;
        }
    }
    public ChunkId DequeueDirtyChunk()
    {
        Queue<ChunkId> dc = dirtyQueueFirst ? firstDirtyChunks : secondDirtyChunks;
        return dc.Dequeue();
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
            var id = FromWorldPos(x, y, z);
            var chunk = ChunkDatas[id];
            chunk[x & GameDefines.CHUNK_MASK, y & GameDefines.CHUNK_MASK, z & GameDefines.CHUNK_MASK] = value;
            EnqueueDirtyChunk(id);
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
            var id = FromWorldPos(index);
            var chunk = ChunkDatas[id];
            chunk[index.x & GameDefines.CHUNK_MASK, index.y & GameDefines.CHUNK_MASK, index.z & GameDefines.CHUNK_MASK] = value;
            EnqueueDirtyChunk(id);
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
        ChunkDatas.Add(id, chunkData);
        var chunkView = go.AddComponent<ChunkView>();
        ChunkViews.Add(id, chunkView);
        EnqueueDirtyChunk(id);
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
        //ComputeShader shader = Resources.Load<ComputeShader>("ComputeShaders/MeshGeneration");
        //Debug.Log(shader.ToString());
    }

    private void Update()
    {
        HandleJobCompletion();
        if (fullUpdate)
        {
            foreach (var p in ChunkDatas)
            {
                EnqueueDirtyChunk(p.Key);
            }
        }
    }
    private void LateUpdate()
    {
        var start = Time.realtimeSinceStartup;
        if (CurrentDirtyQueue.Count > 0 && meshingJob == null)
        {
            DoMeshGeneration();
        }
        var end = Time.realtimeSinceStartup;
        if (fullUpdate)
        {
            Debug.Log($"setup meshing jobs: {end - start}");
            fullUpdate = false;
        }
        dirtyQueueFirst = !dirtyQueueFirst;
    }

    private void DoMeshGeneration()
    {
        switch (MeshingType)
        {
            case 0:
                DoSyncMeshing(CurrentDirtyQueue);
                break;
            case 1:
            case 2:
                meshingJob = new BatchMeshingJob(CurrentDirtyQueue, ChunkDatas, MeshingType);
                break;
            default:
                break;
        }
    }
    private void HandleJobCompletion()
    {
        meshingJob?.HandleJobCompletion();
        if (meshingJob?.IsCompleted == true)
        {
            var meshes = new Mesh[meshingJob.Ids.Count];
            for (int i = 0; i < meshingJob.Ids.Count; i++)
            {
                meshes[i] = ChunkViews[meshingJob.Ids[i]].GetMesh();
            }
            Mesh.ApplyAndDisposeWritableMeshData(meshingJob.Array, meshes);
            var bakingJob = new BatchBakingJob(meshes, meshingJob.Ids);
            bakingJobs.Enqueue(bakingJob);
            meshingJob = null;
        }
        if (bakingJobs.Count > 0)
        {
            var bj = bakingJobs.Dequeue();
            if (bj.Handle.IsCompleted)
            {
                bj.Handle.Complete();
                bj.Ids.Dispose();
                foreach (var id in bj.ChunkIds)
                {
                    ChunkViews[id].SetBakedMesh();
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
}