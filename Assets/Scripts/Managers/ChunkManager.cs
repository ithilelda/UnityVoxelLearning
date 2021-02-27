using System.Collections.Generic;
using System.Linq;
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

    private readonly DoubleQueue<ChunkId> dirtyChunks = new DoubleQueue<ChunkId>();
    private readonly DoubleQueue<IBatchJob> batchJobs = new DoubleQueue<IBatchJob>();

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
            var id = FromWorldPos(x, y, z);
            var chunk = ChunkDatas[id];
            chunk[x & GameDefines.CHUNK_MASK, y & GameDefines.CHUNK_MASK, z & GameDefines.CHUNK_MASK] = value;
            dirtyChunks.Enqueue(id);
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
            dirtyChunks.Enqueue(id);
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
        dirtyChunks.Enqueue(id);
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
                dirtyChunks.Enqueue(p.Key);
            }
        }
    }
    private void LateUpdate()
    {
        var start = Time.realtimeSinceStartup;
        if (dirtyChunks.CurrentCount > 0)
        {
            DoMeshGeneration();
        }
        var end = Time.realtimeSinceStartup;
        if (fullUpdate)
        {
            Debug.Log($"setup meshing jobs: {end - start}");
            fullUpdate = false;
        }
        dirtyChunks.Swap();
    }

    private void DoMeshGeneration()
    {
        switch (MeshingType)
        {
            case 0:
                DoSyncMeshing(dirtyChunks.Current);
                break;
            case 1:
            case 2:
                batchJobs.Enqueue(new BatchMeshingJob(dirtyChunks.Current, MeshingType, this));
                dirtyChunks.ClearCurrent();
                break;
            default:
                break;
        }
    }
    private void HandleJobCompletion()
    {
        while(batchJobs.CurrentCount > 0)
        {
            var bj = batchJobs.Dequeue();
            bj.Run();
            if (bj.IsCompleted)
            {
                var newJob = bj.OnCompletion();
                if (newJob != null) batchJobs.Enqueue(newJob);
            }
            else
            {
                batchJobs.Enqueue(bj);
            }
        }
        batchJobs.Swap();
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