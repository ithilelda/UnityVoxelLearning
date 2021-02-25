using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class ChunkData : MonoBehaviour
{
    public ChunkId ChunkId;
    public ChunkManager ChunkSystem;
    public bool IsDirty;

    public uint[] Voxels = new uint[GameDefines.CHUNK_SIZE_CUBED];

    public static int FlattenIndex(int x, int y, int z) => x * GameDefines.CHUNK_SIZE_SQUARED + y * GameDefines.CHUNK_SIZE + z;
    public static int FlattenIndex(Vector3Int localIndex) => FlattenIndex(localIndex.x, localIndex.y, localIndex.z);
    public static int FlattenIndex(int4 localIndex) => FlattenIndex(localIndex.x, localIndex.y, localIndex.z);

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

    // maps 16 -> 1, -1 -> -1, and 0~15 to 0. If chunk bit is different, then correctly generalizes. (32->1, -1->-1, 0~31->0, etc.)
    public static Vector3Int GetChunkShift(int x, int y, int z) => new Vector3Int((x & GameDefines.CHUNK_ANTI_MASK) >> GameDefines.CHUNK_BIT, (y & GameDefines.CHUNK_ANTI_MASK) >> GameDefines.CHUNK_BIT, (z & GameDefines.CHUNK_ANTI_MASK) >> GameDefines.CHUNK_BIT);
    public static Vector3Int GetChunkShift(Vector3Int localIndex) => GetChunkShift(localIndex.x, localIndex.y, localIndex.z);
    public static int4 GetChunkShift(int4 localIndex) => new int4((localIndex.x & GameDefines.CHUNK_ANTI_MASK) >> GameDefines.CHUNK_BIT, (localIndex.y & GameDefines.CHUNK_ANTI_MASK) >> GameDefines.CHUNK_BIT, (localIndex.z & GameDefines.CHUNK_ANTI_MASK) >> GameDefines.CHUNK_BIT, 0);
    // maps -1 -> 15, 16 -> 0, and 0~15 intact. now correctly generalizes too.
    public static Vector3Int GetShiftedIndex(int x, int y, int z) => new Vector3Int(x & GameDefines.CHUNK_MASK, y & GameDefines.CHUNK_MASK, z & GameDefines.CHUNK_MASK);
    public static Vector3Int GetShiftedIndex(Vector3Int localIndex) => GetShiftedIndex(localIndex.x, localIndex.y, localIndex.z);
    public static int4 GetShiftedIndex(int4 localIndex) => new int4(localIndex.x & GameDefines.CHUNK_MASK, localIndex.y & GameDefines.CHUNK_MASK, localIndex.z & GameDefines.CHUNK_MASK, 0);
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
            return ChunkSystem.ChunkDatas.TryGetValue(ChunkId.Shift(chunkShift), out var chunk) && chunk[GetShiftedIndex(newIndex)] > 0u;
        }
    }
}
