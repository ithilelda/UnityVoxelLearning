using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkView : MonoBehaviour
{
    private MeshFilter filter;

    private int ActualVertexCount = 0;
    private int ActualTriangleCount = 0;

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
    public void AssignMesh(NativeMeshData data)
    {
        //Debug.Log($"vertices count: {data.Indices[0]}, triangles count: {data.Indices[1]}");
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(data.Vertices, 0, data.Indices[0]);
        mesh.SetTriangles(data.Triangles.ToArray(), 0, data.Indices[1], 0);
        mesh.RecalculateNormals();
        filter.mesh = mesh;
    }
    
    public void RenderToMesh(ChunkId id, ChunkData data)
    {
        var mesh = MeshData.GenerateMesh(id, data);
        AssignMesh(mesh);
    }
    public async void RenderToMeshAsync(ChunkId id, ChunkData data)
    {
        var mesh = await Task.Run(() => MeshData.GenerateMesh(id, data));
        AssignMesh(mesh);
    }

    public void RenderToMeshJob(ChunkId id, ChunkData data)
    {
        var cdata = new NativeArray<uint>(GameDefines.CHUNK_SIZE_CUBED, Allocator.TempJob);
        cdata.CopyFrom(data.Voxels);
        var verticesSize = ActualVertexCount == 0 ? GameDefines.INITIAL_VERTEX_ARRAY_COUNT : ActualVertexCount;
        var trianglesSize = ActualTriangleCount == 0 ? GameDefines.INITIAL_TRIANGLE_ARRAY_COUNT : ActualTriangleCount;
        var mesh = new NativeMeshData
        {
            Vertices = new NativeArray<Vector3>(verticesSize, Allocator.TempJob),
            Triangles = new NativeArray<int>(trianglesSize, Allocator.TempJob),
            Indices = new NativeArray<int>(2, Allocator.TempJob)
        };
        var job = new JobMeshGeneration
        {
            Data = cdata,
            MeshData = mesh,
            Id = id
        };
        var handle = job.Schedule();
        handle.Complete();
        ActualVertexCount = mesh.Indices[0] + GameDefines.MESHGEN_ARRAY_HEADROOM;
        ActualTriangleCount = mesh.Indices[1] + GameDefines.MESHGEN_ARRAY_HEADROOM;
        AssignMesh(job.MeshData);
        cdata.Dispose();
        mesh.Vertices.Dispose();
        mesh.Triangles.Dispose();
        mesh.Indices.Dispose();
    }
}
