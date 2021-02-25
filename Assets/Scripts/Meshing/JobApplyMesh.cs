using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

[BurstCompile]
public struct JobApplyMesh : IJob
{
    [ReadOnly]
    public NativeMeshData Data;

    public Mesh.MeshData MeshData;
    
    public void Execute()
    {
        var desc = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp);
        desc[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        desc[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
        desc[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
        MeshData.SetVertexBufferParams(Data.Indices[0], desc);
        desc.Dispose();
        var verts = MeshData.GetVertexData<VertexData>();
        NativeArray<VertexData>.Copy(Data.Vertices, 0, verts, 0, Data.Indices[0]);

        MeshData.SetIndexBufferParams(Data.Indices[1], IndexFormat.UInt32);
        var tris = MeshData.GetIndexData<uint>();
        NativeArray<uint>.Copy(Data.Triangles, 0, tris, 0, Data.Indices[1]);

        MeshData.subMeshCount = 1;
        MeshData.SetSubMesh(0, new SubMeshDescriptor(0, Data.Indices[1]));

    }
}

