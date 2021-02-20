using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public class ChunkSystem : MonoBehaviour
{
    public FastNoiseLite Noise { get; } = new FastNoiseLite();
    public Dictionary<ChunkId, ChunkData> ChunkDatas { get; } = new Dictionary<ChunkId, ChunkData>();
    public Dictionary<ChunkId, ChunkView> ChunkViews { get; } = new Dictionary<ChunkId, ChunkView>();

    [SerializeField, Range(0,3)]
    public int MeshingType;
    public bool fullUpdate;
    public float frequency;

    private Dictionary<ChunkId, JobNaiveCulling> meshGenJobs = new Dictionary<ChunkId, JobNaiveCulling>();
    private Dictionary<ChunkId, JobGreedyMeshing> greedyMeshingJobs = new Dictionary<ChunkId, JobGreedyMeshing>();
    private Dictionary<ChunkId, JobHandle> handles = new Dictionary<ChunkId, JobHandle>();

    private NativeArray<uint> emptyChunk;


    public static ChunkId FromWorldPos(int x, int y, int z)
    {
        return new ChunkId(x >> GameDefines.CHUNK_BIT, y >> GameDefines.CHUNK_BIT, z >> GameDefines.CHUNK_BIT);
    }
    public static Vector3 ToWorldPos(ChunkId id, int x, int y, int z)
    {
        return new Vector3((id.x << GameDefines.CHUNK_BIT) + x, (id.y << GameDefines.CHUNK_BIT) + y, (id.z << GameDefines.CHUNK_BIT) + z);
    }
    public static Vector3 ToWorldPos(ChunkId id, int4 start)
    {
        return new Vector3((id.x << GameDefines.CHUNK_BIT) + start.x, (id.y << GameDefines.CHUNK_BIT) + start.y, (id.z << GameDefines.CHUNK_BIT) + start.z);
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
        }
    }

    public void GenerateChunk(ChunkId id)
    {
        var go = new GameObject($"Chunk {id.x} {id.y} {id.z}");
        go.transform.parent = transform.parent;
        var chunkData = go.AddComponent<ChunkData>();
        chunkData.ChunkId = id;
        chunkData.ChunkSystem = this;
        for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    var pos = ToWorldPos(id, x, y, z);
                    //Debug.Log(pos);
                    chunkData[x, y, z] = (uint)(Mathf.FloorToInt(Noise.GetNoise(pos.x, pos.y, pos.z)) + 1);
                }
            }
        }
        chunkData.IsDirty = true;
        ChunkDatas.Add(id, chunkData);
        var chunkView = go.AddComponent<ChunkView>();
        ChunkViews.Add(id, chunkView);
    }

    private void Awake()
    {
        emptyChunk = new NativeArray<uint>(GameDefines.CHUNK_SIZE_CUBED, Allocator.Persistent);
    }
    private void OnDestroy()
    {
        emptyChunk.Dispose();
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
        ComputeShader shader = Resources.Load<ComputeShader>("Shaders/MeshGeneration");
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
        foreach (var p in ChunkViews)
        {
            if(handles.TryGetValue(p.Key, out var handle))
            {
                if (handle.IsCompleted)
                {
                    var view = p.Value;
                    handle.Complete();
                    if (meshGenJobs.TryGetValue(p.Key, out var meshgen))
                    {
                        view.AssignMesh(meshgen.MeshData);
                        meshgen.Dispose();
                        meshGenJobs.Remove(p.Key);
                    }
                    if (greedyMeshingJobs.TryGetValue(p.Key, out var greedy))
                    {
                        view.AssignMesh(greedy.MeshData);
                        greedy.Dispose();
                        greedyMeshingJobs.Remove(p.Key);
                    }

                    handles.Remove(p.Key);
                }
            }
        }
        var end = Time.realtimeSinceStartup;
        if (fullUpdate)
        {
            Debug.Log(end - start);
            fullUpdate = false;
        }
    }

    private void DoMeshGeneration()
    {
        switch (MeshingType)
        {
            case 0:
                foreach (var p in ChunkDatas)
                {
                    if (p.Value.IsDirty)
                    {
                        var view = ChunkViews[p.Key];
                        view.RenderToMesh(p.Key, p.Value);
                        p.Value.IsDirty = false;
                    }
                }
                break;
            case 1:
                foreach (var p in ChunkDatas)
                {
                    if (p.Value.IsDirty)
                    {
                        var view = ChunkViews[p.Key];
                        view.RenderToMeshAsync(p.Key, p.Value);
                        p.Value.IsDirty = false;
                    }
                }
                break;
            case 2:
                ScheduleMeshJob();
                break;
            case 3:
                ScheduleGreedyMeshingJob();
                break;
        }
    }

    private void ScheduleMeshJob()
    {
        foreach (var p in ChunkDatas)
        {
            if (p.Value.IsDirty)
            {
                var mesh = new NativeMeshData
                {
                    Vertices = new NativeArray<Vector3>(GameDefines.MAXIMUM_VERTEX_ARRAY_COUNT, Allocator.TempJob),
                    Triangles = new NativeArray<int>(GameDefines.MAXIMUM_TRIANGLE_ARRAY_COUNT, Allocator.TempJob),
                    Indices = new NativeArray<int>(2, Allocator.TempJob)
                };
                var data = MeshHelper.GetChunkWithPerimeterForJob(ChunkDatas, p.Key);
                var job = new JobNaiveCulling
                {
                    Data = data,
                    MeshData = mesh,
                    Id = p.Key
                };
                meshGenJobs.Add(p.Key, job);
                handles[p.Key] = job.Schedule();
                p.Value.IsDirty = false;
            }
        }
        JobHandle.ScheduleBatchedJobs();
    }
    private void ScheduleGreedyMeshingJob()
    {
        foreach (var p in ChunkDatas)
        {
            if (p.Value.IsDirty)
            {
                var mesh = new NativeMeshData
                {
                    Vertices = new NativeArray<Vector3>(GameDefines.MAXIMUM_VERTEX_ARRAY_COUNT, Allocator.TempJob),
                    Triangles = new NativeArray<int>(GameDefines.MAXIMUM_TRIANGLE_ARRAY_COUNT, Allocator.TempJob),
                    Indices = new NativeArray<int>(2, Allocator.TempJob)
                };
                var data = new NativeArray<uint>(GameDefines.CHUNK_PERIMETER_SIZE, Allocator.TempJob);
                var id = p.Key;
                var hasleft = ChunkDatas.TryGetValue(id.Shift(Vector3Int.left), out var left);
                var hasright = ChunkDatas.TryGetValue(id.Shift(Vector3Int.right), out var right);
                var hastop = ChunkDatas.TryGetValue(id.Shift(Vector3Int.up), out var top);
                var hasbottom = ChunkDatas.TryGetValue(id.Shift(Vector3Int.down), out var bottom);
                var hasfront = ChunkDatas.TryGetValue(id.Shift(Vector3Int.forward), out var front);
                var hasback = ChunkDatas.TryGetValue(id.Shift(Vector3Int.back), out var back);
                var setupJob = new JobSetupPerimeterChunkData
                {
                    Current = p.Value.Voxels,
                    HasLeft = hasleft,
                    Left = hasleft ? left.Voxels : emptyChunk,
                    HasRight = hasright,
                    Right = hasright ? right.Voxels : emptyChunk,
                    HasTop = hastop,
                    Top = hastop ? top.Voxels : emptyChunk,
                    HasBottom = hasbottom,
                    Bottom = hasbottom ? bottom.Voxels : emptyChunk,
                    HasFront = hasfront,
                    Front = hasfront ? front.Voxels : emptyChunk,
                    HasBack = hasback,
                    Back = hasback ? back.Voxels : emptyChunk,
                    Output = data
                };
                var greedyJob = new JobGreedyMeshing
                {
                    Data = data,
                    MeshData = mesh,
                    Id = p.Key
                };
                greedyMeshingJobs.Add(p.Key, greedyJob);
                handles[p.Key] = greedyJob.Schedule(setupJob.Schedule());
                p.Value.IsDirty = false;
            }
        }
        JobHandle.ScheduleBatchedJobs();
    }
}