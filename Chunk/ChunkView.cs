using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkView : MonoBehaviour
{
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
    private MeshFilter filter;

    public ChunkId ChunkId;

    // Start is called before the first frame update
    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        filter.mesh = new Mesh();
    }

    public void AssignMesh(MeshData data)
    {
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(data.Vertices);
        mesh.SetTriangles(data.Triangles.ToArray(), 0);
        mesh.RecalculateNormals();
        filter.mesh = mesh;
    }
    public MeshData GenerateMesh(ChunkData chunkData)
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
                        var pos = ChunkSystem.ToWorldPos(ChunkId, x, y, z);
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
    public MeshData RenderToMesh(ChunkData data)
    {
        return GenerateMesh(data);
    }
    public Task<MeshData> RenderToMeshAsync(ChunkData data)
    {
        return Task.Run(() => GenerateMesh(data));
    }
}
