using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkData : MonoBehaviour
{
    public ChunkId ChunkId;
    public ChunkSystem ChunkSystem;
    public bool IsDirty;

    public uint[] Voxels { get; } = new uint[GameDefines.CHUNK_SIZE_CUBED];

    public static int FlattenIndex(Vector3Int index) => index.x * GameDefines.CHUNK_SIZE_SQUARED + index.y * GameDefines.CHUNK_SIZE + index.z;
    public static int FlattenIndex(int x, int y, int z) => x * GameDefines.CHUNK_SIZE_SQUARED + y * GameDefines.CHUNK_SIZE + z;
    public static int GetX(int index) => index >> (GameDefines.CHUNK_BIT * 2);
    public static int GetY(int index) => index >> GameDefines.CHUNK_BIT;
    public static int GetZ(int index) => index & GameDefines.CHUNK_MASK;

    public uint this[int i]
    {
        get { return Voxels[i]; }
        set { Voxels[i] = value; }
    }
    public uint this[int x, int y, int z]
    {
        get { return Voxels[FlattenIndex(x, y, z)]; }
        set { Voxels[FlattenIndex(x, y, z)] = value; }
    }

    public uint this[Vector3Int index]
    {
        get { return Voxels[FlattenIndex(index)]; }
        set { Voxels[FlattenIndex(index)] = value; }
    }

    public static Vector3Int GetChunkShift(Vector3Int index) => new Vector3Int((index.x & -16) / 16, (index.y & -16) / 16, (index.z & -16) / 16); // maps 16 -> 1, -1 -> -1, and 0~15 to 0.
    public static Vector3Int GetChunkShift(int x, int y, int z) => new Vector3Int((x & -16) / 16, (y & -16) / 16, (z & -16) / 16); // maps 16 -> 1, -1 -> -1, and 0~15 to 0.
    public static Vector3Int GetActualIndex(Vector3Int index) => new Vector3Int(index.x & 15, index.y & 15, index.z & 15); // maps -1 -> 15, 16 -> 0, and 0~15 intact.
    public static Vector3Int GetActualIndex(int x, int y, int z) => new Vector3Int(x & 15, y & 15, z & 15); // maps -1 -> 15, 16 -> 0, and 0~15 intact.
    public bool HasAdjacency(int x, int y, int z, Vector3Int direction)
    {
        return HasAdjacency(new Vector3Int(x, y, z), direction);
    }
    public bool HasAdjacency(Vector3Int index, Vector3Int direction)
    {
        var newIndex = index + direction;
        var chunkShift = GetChunkShift(newIndex);
        if (chunkShift.Equals(Vector3Int.zero))
        {
            return this[newIndex] > 0u;
        }
        else
        {
            return ChunkSystem.ChunkDatas.TryGetValue(ChunkId.Shift(chunkShift), out var chunk) && chunk[GetActualIndex(newIndex)] > 0u;
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {

    }

}
