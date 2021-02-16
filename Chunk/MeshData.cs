using System.Collections.Generic;
using UnityEngine;


public class MeshData
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<int> Triangles = new List<int>();


    private static Vector3[] cubeVertices = new[] {
        Vector3.zero,
        Vector3.up,
        Vector3.up + Vector3.right,
        Vector3.right,
        Vector3.forward,
        Vector3.up + Vector3.forward,
        Vector3.one,
        Vector3.right + Vector3.forward,
    };
    public static MeshData GenerateMesh(ChunkId id, ChunkData chunkData)
    {
        var ret = new MeshData();

        for (var x = 0; x < GameDefines.CHUNK_SIZE; x++)
        {
            for (var y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                for (var z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    var index = new Vector3Int(x, y, z);
                    var voxelType = chunkData[index];
                    if (voxelType > 0u)
                    {
                        var pos = ChunkSystem.ToWorldPos(id, x, y, z);
                        if (!chunkData.HasAdjacency(index, Vector3Int.forward))
                        {
                            var cp = ret.Vertices.Count;
                            ret.Vertices.Add(pos + cubeVertices[4]);
                            ret.Vertices.Add(pos + cubeVertices[7]);
                            ret.Vertices.Add(pos + cubeVertices[6]);
                            ret.Vertices.Add(pos + cubeVertices[5]);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 1);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 3);
                        }
                        if (!chunkData.HasAdjacency(index, Vector3Int.back))
                        {
                            var cp = ret.Vertices.Count;
                            ret.Vertices.Add(pos + cubeVertices[0]);
                            ret.Vertices.Add(pos + cubeVertices[1]);
                            ret.Vertices.Add(pos + cubeVertices[2]);
                            ret.Vertices.Add(pos + cubeVertices[3]);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 1);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 3);
                        }
                        if (!chunkData.HasAdjacency(index, Vector3Int.up))
                        {
                            var cp = ret.Vertices.Count;
                            ret.Vertices.Add(pos + cubeVertices[1]);
                            ret.Vertices.Add(pos + cubeVertices[5]);
                            ret.Vertices.Add(pos + cubeVertices[6]);
                            ret.Vertices.Add(pos + cubeVertices[2]);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 1);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 3);
                        }
                        if (!chunkData.HasAdjacency(index, Vector3Int.down))
                        {
                            var cp = ret.Vertices.Count;
                            ret.Vertices.Add(pos + cubeVertices[0]);
                            ret.Vertices.Add(pos + cubeVertices[3]);
                            ret.Vertices.Add(pos + cubeVertices[7]);
                            ret.Vertices.Add(pos + cubeVertices[4]);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 1);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 3);
                        }
                        if (!chunkData.HasAdjacency(index, Vector3Int.right))
                        {
                            var cp = ret.Vertices.Count;
                            ret.Vertices.Add(pos + cubeVertices[2]);
                            ret.Vertices.Add(pos + cubeVertices[6]);
                            ret.Vertices.Add(pos + cubeVertices[7]);
                            ret.Vertices.Add(pos + cubeVertices[3]);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 1);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 3);
                        }
                        if (!chunkData.HasAdjacency(index, Vector3Int.left))
                        {
                            var cp = ret.Vertices.Count;
                            ret.Vertices.Add(pos + cubeVertices[0]);
                            ret.Vertices.Add(pos + cubeVertices[4]);
                            ret.Vertices.Add(pos + cubeVertices[5]);
                            ret.Vertices.Add(pos + cubeVertices[1]);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 1);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 0);
                            ret.Triangles.Add(cp + 2);
                            ret.Triangles.Add(cp + 3);
                        }
                    }
                }
            }
        }
        return ret;
    }
}
