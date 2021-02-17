using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;

public class ChunkSystem : MonoBehaviour
{
    public FastNoiseLite Noise { get; } = new FastNoiseLite();
    public Dictionary<ChunkId, ChunkData> ChunkDatas { get; } = new Dictionary<ChunkId, ChunkData>();
    public Dictionary<ChunkId, ChunkView> ChunkViews { get; } = new Dictionary<ChunkId, ChunkView>();

    public Material material;
    [SerializeField, Range(0,2)]
    public int MeshingType;
    public bool fullUpdate;
    public float frequency;

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

    public static ChunkId FromWorldPos(int x, int y, int z)
    {
        return new ChunkId(x >> GameDefines.CHUNK_BIT, y >> GameDefines.CHUNK_BIT, z >> GameDefines.CHUNK_BIT);
    }

    public static Vector3 ToWorldPos(ChunkId id, int x, int y, int z)
    {
        return new Vector3((id.x << GameDefines.CHUNK_BIT) + x, (id.y << GameDefines.CHUNK_BIT) + y, (id.z << GameDefines.CHUNK_BIT) + z);
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
            for (int y = 0; y < 10; y++)
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
                foreach (var p in ChunkDatas)
                {
                    if (p.Value.IsDirty)
                    {
                        var view = ChunkViews[p.Key];
                        view.RenderToMeshJob(p.Key, p.Value);
                        p.Value.IsDirty = false;
                    }
                }
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
    }
}