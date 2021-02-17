using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;


[BurstCompile]
public struct JobMeshGeneration : IJob
{
    [ReadOnly]
    public NativeArray<uint> Data;
    [ReadOnly]
    public ChunkId Id;

    public NativeMeshData MeshData;

    public void Dispose()
    {
        Data.Dispose();
        MeshData.Dispose();
    }

    public void Execute()
    {
        for (var x = 0; x < GameDefines.CHUNK_SIZE; x++)
        {
            for (var y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                for (var z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    var voxelType = Data[ChunkData.FlattenIndex(x, y, z)];
                    if (voxelType > 0u)
                    {
                        var pos = ChunkSystem.ToWorldPos(Id, x, y, z);
                        if (!HasAdjacency(x, y, z, Vector3Int.forward))
                        {
                            var cp = MeshData.Indices[0];
                            MeshData.AddVertex(pos + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.right + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.one);
                            MeshData.AddVertex(pos + Vector3.up + Vector3.forward);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 1);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 3);
                        }
                        if (!HasAdjacency(x, y, z, Vector3Int.back))
                        {
                            var cp = MeshData.Indices[0];
                            MeshData.AddVertex(pos + Vector3.zero);
                            MeshData.AddVertex(pos + Vector3.up);
                            MeshData.AddVertex(pos + Vector3.up + Vector3.right);
                            MeshData.AddVertex(pos + Vector3.right);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 1);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 3);
                        }
                        if (!HasAdjacency(x, y, z, Vector3Int.up))
                        {
                            var cp = MeshData.Indices[0];
                            MeshData.AddVertex(pos + Vector3.up);
                            MeshData.AddVertex(pos + Vector3.up + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.one);
                            MeshData.AddVertex(pos + Vector3.up + Vector3.right);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 1);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 3);
                        }
                        if (!HasAdjacency(x, y, z, Vector3Int.down))
                        {
                            var cp = MeshData.Indices[0];
                            MeshData.AddVertex(pos + Vector3.zero);
                            MeshData.AddVertex(pos + Vector3.right);
                            MeshData.AddVertex(pos + Vector3.right + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.forward);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 1);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 3);
                        }
                        if (!HasAdjacency(x, y, z, Vector3Int.right))
                        {
                            var cp = MeshData.Indices[0];
                            MeshData.AddVertex(pos + Vector3.up + Vector3.right);
                            MeshData.AddVertex(pos + Vector3.one);
                            MeshData.AddVertex(pos + Vector3.right + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.right);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 1);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 3);
                        }
                        if (!HasAdjacency(x, y, z, Vector3Int.left))
                        {
                            var cp = MeshData.Indices[0];
                            MeshData.AddVertex(pos + Vector3.zero);
                            MeshData.AddVertex(pos + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.up + Vector3.forward);
                            MeshData.AddVertex(pos + Vector3.up);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 1);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 0);
                            MeshData.AddTriangle(cp + 2);
                            MeshData.AddTriangle(cp + 3);
                        }
                    }
                }
            }
        }
        //Debug.Log($"inside job: {MeshData.Indices[0]}, {MeshData.Indices[1]}");
    }

    private bool HasAdjacency(int x, int y, int z, Vector3Int direction)
    {
        var chunkShift = ChunkData.GetChunkShift(x + direction.x, y + direction.y, z + direction.z);
        if (chunkShift.Equals(Vector3Int.zero))
        {
            return Data[ChunkData.FlattenIndex(x, y, z) + ChunkData.FlattenIndex(direction)] > 0u;
        }
        else if (chunkShift.Equals(Vector3Int.left))
        {
            //if we are asking for perimeter blocks on the left.
            var bi = GameDefines.CHUNK_SIZE_CUBED;
            return Data[bi + ChunkData.FlattenIndex(0, y, z)] > 0u;
        }
        else if (chunkShift.Equals(Vector3Int.right))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED;
            return Data[bi + ChunkData.FlattenIndex(0, y, z)] > 0u;
        }
        else if (chunkShift.Equals(Vector3Int.down))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 2;
            return Data[bi + ChunkData.FlattenIndex(0, x, z)] > 0u;
        }
        else if (chunkShift.Equals(Vector3Int.up))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 3;
            return Data[bi + ChunkData.FlattenIndex(0, x, z)] > 0u;
        }
        else if (chunkShift.Equals(Vector3Int.back))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 4;
            return Data[bi + ChunkData.FlattenIndex(0, x, y)] > 0u;
        }
        else if (chunkShift.Equals(Vector3Int.forward))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 5;
            return Data[bi + ChunkData.FlattenIndex(0, x, y)] > 0u;
        }
        else
        {
            return false;
        }
    }
}
