using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkViewDeprec : MonoBehaviour
{
    private Vector3[] _cubeVertices = new[] {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 1, 1),
        new Vector3(1, 1, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 0, 1),
    };
    private int[] _cubeTriangles = new[] {
    // Front
	    0, 2, 1,
        0, 3, 2,
    // Top
	    2, 3, 4,
        2, 4, 5,
    // Right
	    1, 2, 5,
        1, 5, 6,
    // Left
	    0, 7, 4,
        0, 4, 3,
    // Back
	    5, 4, 7,
        5, 7, 6,
    // Bottom
	    0, 6, 7,
        0, 1, 6
    };

    public ChunkId ChunkId;
    public ChunkData ChunkData;
    public ChunkSystem ChunkSystem;

    private MeshFilter filter;


    // Start is called before the first frame update
    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        filter.mesh = new Mesh();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void RenderToMesh()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (var x = 0; x < 16; x++)
        {
            for (var y = 0; y < 16; y++)
            {
                for (var z = 0; z < 16; z++)
                {
                    var voxelType = ChunkData[x, y, z];
                    // If it is air we ignore this block
                    if (voxelType == 0)
                        continue;
                    var pos = ChunkSystem.ToWorldPos(ChunkId, x, y, z);
                    // Remember current position in vertices list so we can add triangles relative to that
                    var verticesPos = vertices.Count;
                    foreach (var vert in _cubeVertices)
                        vertices.Add(pos + vert); // Voxel postion + cubes vertex
                    foreach (var tri in _cubeTriangles)
                        triangles.Add(verticesPos + tri); // Position in vertices list for new vertex we just added
                }
            }
        }
        // Apply new mesh to MeshFilter
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles.ToArray(), 0);
        filter.mesh = mesh;
    }
}
