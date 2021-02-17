using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class ChunkSystem : MonoBehaviour
{
    public FastNoiseLite Noise { get; } = new FastNoiseLite();
    public Dictionary<ChunkId, ChunkData> ChunkDatas { get; } = new Dictionary<ChunkId, ChunkData>();
    public Dictionary<ChunkId, ChunkView> ChunkViews { get; } = new Dictionary<ChunkId, ChunkView>();

    [SerializeField, Range(0,2)]
    public int MeshingType;
    public bool fullUpdate;
    public float frequency;
    public Material material;

    private Dictionary<ChunkId, JobMeshGeneration> jobs = new Dictionary<ChunkId, JobMeshGeneration>();
    private Dictionary<ChunkId, JobHandle> handles = new Dictionary<ChunkId, JobHandle>();

    public static ChunkId FromWorldPos(int x, int y, int z)
    {
        return new ChunkId(x >> GameDefines.CHUNK_BIT, y >> GameDefines.CHUNK_BIT, z >> GameDefines.CHUNK_BIT);
    }
    public static Vector3 ToWorldPos(ChunkId id, int x, int y, int z)
    {
        return new Vector3((id.x << GameDefines.CHUNK_BIT) + x, (id.y << GameDefines.CHUNK_BIT) + y, (id.z << GameDefines.CHUNK_BIT) + z);
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

    public NativeArray<uint> GetChunkWithPerimeterForJob(ChunkId id)
    {
        var ret = new NativeArray<uint>(GameDefines.CHUNK_PERIMETER_SIZE_CUBED, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        var curChunk = ChunkDatas[id];
        // we first copy the entire chunk data array into the native array contiguously. This is the data we are going to loop through.
        NativeArray<uint>.Copy(curChunk.Voxels, ret, GameDefines.CHUNK_SIZE_CUBED);
        // then, for every neighbor chunk, we fill in the destination space with data. If the chunk is not loaded, we skip the face and leave the number as the default value indicating air block.
        if (ChunkDatas.TryGetValue(id.Shift(Vector3Int.left), out curChunk))
        {
            // we need the right face blocks(x = CHUNK_SIZE - 1, or known as CHUNK_MASK) of the left chunk.
            // since they are contiguous in our chunk data array, we can just memcpy them.
            NativeArray<uint>.Copy(curChunk.Voxels, ChunkData.FlattenIndex(GameDefines.CHUNK_MASK, 0, 0), ret, GameDefines.CHUNK_SIZE_CUBED, GameDefines.CHUNK_SIZE_SQUARED);
        }
        if (ChunkDatas.TryGetValue(id.Shift(Vector3Int.right), out curChunk))
        {
            // we need the left face blocks(x = 0) of the right chunk, you get the idea.
            // since they are contiguous in our chunk data array, we can just memcpy them.
            NativeArray<uint>.Copy(curChunk.Voxels, 0, ret, GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED, GameDefines.CHUNK_SIZE_SQUARED);
        }
        // filling in the y shifted chunk datas.
        if (ChunkDatas.TryGetValue(id.Shift(Vector3Int.down), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 2;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    ret[bi + outer + z] = curChunk[x, GameDefines.CHUNK_MASK, z];
                }
            }
        }
        if (ChunkDatas.TryGetValue(id.Shift(Vector3Int.up), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 3;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    ret[bi + outer + z] = curChunk[x, 0, z];
                }
            }
        }
        // filling in the z shifted chunk datas.
        if (ChunkDatas.TryGetValue(id.Shift(Vector3Int.back), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 4;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
                {
                    ret[bi + outer + y] = curChunk[x, y, GameDefines.CHUNK_MASK];
                }
            }
        }
        if (ChunkDatas.TryGetValue(id.Shift(Vector3Int.forward), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 5;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
                {
                    ret[bi + outer + y] = curChunk[x, y, 0];
                }
            }
        }

        return ret;
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
        chunkView.GetComponent<MeshRenderer>().material = material;
        ChunkViews.Add(id, chunkView);
    }

    private void Start()
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(frequency);
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 1; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    var id = new ChunkId(x, y, z);
                    GenerateChunk(id);
                }
            }
        }
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
        switch (MeshingType)
        {
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
            default:
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
        }
        var end = Time.realtimeSinceStartup;
        if (fullUpdate)
        {
            Debug.Log(end - start);
            fullUpdate = false;
        }
        foreach(var p in ChunkViews)
        {
            if(handles.TryGetValue(p.Key, out var handle))
            {
                if (handle.IsCompleted)
                {
                    var view = p.Value;
                    handle.Complete();
                    var job = jobs[p.Key];
                    view.ActualVertexCount = job.MeshData.Indices[0] + GameDefines.MESHGEN_ARRAY_HEADROOM;
                    view.ActualTriangleCount = job.MeshData.Indices[1] + GameDefines.MESHGEN_ARRAY_HEADROOM;
                    view.AssignMesh(job.MeshData);
                    job.Dispose();
                    jobs.Remove(p.Key);
                    handles.Remove(p.Key);
                }
            }
        }
    }

    private void ScheduleMeshJob()
    {
        foreach (var p in ChunkDatas)
        {
            if (p.Value.IsDirty)
            {
                var view = ChunkViews[p.Key];
                var verticesSize = view.ActualVertexCount == 0 ? GameDefines.INITIAL_VERTEX_ARRAY_COUNT : view.ActualVertexCount;
                var trianglesSize = view.ActualTriangleCount == 0 ? GameDefines.INITIAL_TRIANGLE_ARRAY_COUNT : view.ActualTriangleCount;
                var mesh = new NativeMeshData
                {
                    Vertices = new NativeArray<Vector3>(verticesSize, Allocator.TempJob),
                    Triangles = new NativeArray<int>(trianglesSize, Allocator.TempJob),
                    Indices = new NativeArray<int>(2, Allocator.TempJob)
                };
                var job = new JobMeshGeneration
                {
                    Data = GetChunkWithPerimeterForJob(p.Key),
                    MeshData = mesh,
                    Id = p.Key
                };
                jobs.Add(p.Key, job);
                handles[p.Key] = job.Schedule();
                p.Value.IsDirty = false;
            }
        }
        JobHandle.ScheduleBatchedJobs();
    }
}