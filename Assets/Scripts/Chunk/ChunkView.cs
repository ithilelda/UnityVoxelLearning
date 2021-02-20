using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkView : MonoBehaviour
{
    private MeshFilter filter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public TextureManager textureManager;

    private void Awake()
    {
        textureManager = GameObject.Find("TextureManager").GetComponent<TextureManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        filter.mesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.enabled = true;
        meshRenderer = GetComponent<MeshRenderer>();
        var material = Resources.Load<Material>("Materials/Voxel");
        material.SetTexture("VoxelTextures", textureManager.VoxelTextures);
        meshRenderer.material = material;
    }

    public void AssignMesh(MeshData data)
    {
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(data.Vertices);
        mesh.SetTriangles(data.Triangles.ToArray(), 0);
        mesh.RecalculateNormals();
        filter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    public void AssignMesh(NativeMeshData data)
    {
        //Debug.Log($"vertices count: {data.Indices[0]}, triangles count: {data.Indices[1]}");
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(data.Vertices, 0, data.Indices[0]);
        mesh.SetTriangles(data.Triangles.ToArray(), 0, data.Indices[1], 0);
        mesh.SetUVs(0, data.UVs, 0, data.Indices[2]);
        mesh.RecalculateNormals();
        filter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
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

}
