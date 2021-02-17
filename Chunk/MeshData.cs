using System.Collections.Generic;
using UnityEngine;


public class MeshData
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<int> Triangles = new List<int>();

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
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[4]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[7]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[6]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[5]);
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
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[0]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[1]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[2]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[3]);
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
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[1]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[5]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[6]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[2]);
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
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[0]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[3]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[7]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[4]);
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
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[2]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[6]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[7]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[3]);
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
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[0]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[4]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[5]);
                            ret.Vertices.Add(pos + GameDefines.CubeVertices[1]);
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
