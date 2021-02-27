using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

[BurstCompile]
public struct JobApplyMesh : IJob
{
    [ReadOnly]
    public NativeMeshData InputData;

    public Mesh.MeshData OutputData;
    
    public void Execute()
    {
        var desc = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp);
        desc[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        desc[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
        desc[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
        OutputData.SetVertexBufferParams(InputData.Indices[0], desc);
        desc.Dispose();
        var verts = OutputData.GetVertexData<VertexData>();
        NativeArray<VertexData>.Copy(InputData.Vertices, 0, verts, 0, InputData.Indices[0]);

        OutputData.SetIndexBufferParams(InputData.Indices[1], IndexFormat.UInt32);
        var tris = OutputData.GetIndexData<uint>();
        NativeArray<uint>.Copy(InputData.Triangles, 0, tris, 0, InputData.Indices[1]);

        OutputData.subMeshCount = 1;
        OutputData.SetSubMesh(0, new SubMeshDescriptor(0, InputData.Indices[1]));

    }
}

