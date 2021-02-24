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
    public static readonly VertexAttributeDescriptor[] VertexAttributes = new VertexAttributeDescriptor[]
    {
        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
    };

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
        filter.mesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.enabled = true;
        meshRenderer = GetComponent<MeshRenderer>();
        var material = Resources.Load<Material>("Materials/Voxel");
        material.SetTexture("_VoxelTextures", textureManager.VoxelTextures);
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
        //Debug.Log($"{data.Indices[0]}, {data.Indices[1]}");
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertexBufferParams(data.Indices[0], VertexAttributes);
        mesh.SetVertexBufferData(data.Vertices, 0, 0, data.Indices[0]);
        mesh.SetIndexBufferParams(data.Indices[1], IndexFormat.UInt32);
        mesh.SetIndexBufferData(data.Triangles, 0, 0, data.Indices[1]);
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, data.Indices[1]));
        filter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    
    public void RenderToMesh(ChunkId id, ChunkData data)
    {
        var mesh = MeshData.GenerateMesh(id, data);
        AssignMesh(mesh);
    }
}
