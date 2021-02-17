using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkData : MonoBehaviour
{
    public ChunkId ChunkId;
    public ChunkSystem ChunkSystem;
    public bool IsDirty;

    public uint[] Voxels { get; } = new uint[GameDefines.CHUNK_SIZE_CUBED];

    public static int FlattenIndex(Vector3Int localIndex) => localIndex.x * GameDefines.CHUNK_SIZE_SQUARED + localIndex.y * GameDefines.CHUNK_SIZE + localIndex.z;
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

    public uint this[Vector3Int localIndex]
    {
        get { return Voxels[FlattenIndex(localIndex)]; }
        set { Voxels[FlattenIndex(localIndex)] = value; }
    }

    public static Vector3Int GetChunkShift(Vector3Int localIndex) => new Vector3Int((localIndex.x & -16) / 16, (localIndex.y & -16) / 16, (localIndex.z & -16) / 16); // maps 16 -> 1, -1 -> -1, and 0~15 to 0.
    public static Vector3Int GetChunkShift(int x, int y, int z) => new Vector3Int((x & -16) / 16, (y & -16) / 16, (z & -16) / 16); // maps 16 -> 1, -1 -> -1, and 0~15 to 0.
    public static Vector3Int GetActualIndex(Vector3Int localIndex) => new Vector3Int(localIndex.x & 15, localIndex.y & 15, localIndex.z & 15); // maps -1 -> 15, 16 -> 0, and 0~15 intact.
    public static Vector3Int GetActualIndex(int x, int y, int z) => new Vector3Int(x & 15, y & 15, z & 15); // maps -1 -> 15, 16 -> 0, and 0~15 intact.
    public bool HasAdjacency(int x, int y, int z, Vector3Int direction)
    {
        return HasAdjacency(new Vector3Int(x, y, z), direction);
    }
    public bool HasAdjacency(Vector3Int localIndex, Vector3Int direction)
    {
        var newIndex = localIndex + direction;
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
}
