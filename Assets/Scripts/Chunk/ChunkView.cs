using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkView : MonoBehaviour
{
    public TextureManager textureManager;

    private MeshFilter filter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private void Awake()
    {
        textureManager = GameObject.Find("TextureManager").GetComponent<TextureManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        var material = Resources.Load<Material>("Materials/Voxel");
        material.SetTexture("_VoxelTextures", textureManager.VoxelTextures);
        meshRenderer.material = material;
    }

    public Mesh GetMesh() => filter.mesh;
    public void SetBakedMesh() => meshCollider.sharedMesh = filter.mesh;

    public void AssignMesh(MeshData data)
    {
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(data.Vertices);
        mesh.SetTriangles(data.Triangles.ToArray(), 0);
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }    
    public void RenderToMesh(ChunkData data)
    {
        var mesh = MeshData.GenerateMesh(data);
        AssignMesh(mesh);
    }
}
