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

    public void Execute()
    {
        for (var x = 0; x < GameDefines.CHUNK_SIZE; x++)
        {
            var xi = x * GameDefines.CHUNK_SIZE_SQUARED;
            for (var y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                var yi = xi + y * GameDefines.CHUNK_SIZE;
                for (var z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    var index = yi + z;
                    var voxelType = Data[index];
                    if (voxelType > 0u)
                    {
                        var pos = ChunkSystem.ToWorldPos(Id, x, y, z);
                        if (!HasAdjacency(index, x, y, z, Vector3Int.forward))
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
                        if (!HasAdjacency(index, x, y, z, Vector3Int.back))
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
                        if (!HasAdjacency(index, x, y, z, Vector3Int.up))
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
                        if (!HasAdjacency(index, x, y, z, Vector3Int.down))
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
                        if (!HasAdjacency(index, x, y, z, Vector3Int.right))
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
                        if (!HasAdjacency(index, x, y, z, Vector3Int.left))
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

    private bool HasAdjacency(int index, int x, int y, int z, Vector3Int direction)
    {
        var chunkShift = ChunkData.GetChunkShift(x + direction.x, y + direction.y, z + direction.z);
        if (chunkShift.Equals(Vector3Int.zero))
        {
            return Data[index + ChunkData.FlattenIndex(direction)] > 0u;
        }
        else
        {
            return false;
        }
    }
}
