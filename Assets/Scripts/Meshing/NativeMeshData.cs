using UnityEngine;
using Unity.Collections;


public partial struct NativeMeshData
{
    public NativeArray<VertexData> Vertices;
    public NativeArray<uint> Triangles;
    public NativeArray<int> Indices;

    public void Dispose()
    {
        Vertices.Dispose();
        Triangles.Dispose();
        Indices.Dispose();
    }

    public void AddVertex(int index, Vector3 position, Vector3 normal, Vector2 uv)
    {
        var curV = Vertices[index];
        curV.Position = position;
        curV.Normal = normal;
        curV.UV = uv;
        Vertices[index] = curV;
    }
    public void AddFace(Vector3 offset, Facing facing, Vector3 size)
    {
        uint cp = (uint)Indices[0];
        Triangles[Indices[1]++] = cp + 0;
        Triangles[Indices[1]++] = cp + 1;
        Triangles[Indices[1]++] = cp + 2;
        Triangles[Indices[1]++] = cp + 0;
        Triangles[Indices[1]++] = cp + 2;
        Triangles[Indices[1]++] = cp + 3;

        switch (facing)
        {
            case Facing.FRONT:
                // to understand all these cryptic vector math, just draw a cube and set the bottom left inner most corner as your origin
                // You will see the 4 vertices each face has, and how the size vector's elements should be applied.
                AddVertex(Indices[0]++, offset + Vector3.forward, Vector3.forward, Vector2.zero);
                AddVertex(Indices[0]++, offset + Vector3.right * size.x + Vector3.forward, Vector3.forward, Vector2.right * size.x);
                AddVertex(Indices[0]++, offset + Vector3.right * size.x + Vector3.up * size.y + Vector3.forward, Vector3.forward, Vector2.right * size.x + Vector2.up * size.y);
                AddVertex(Indices[0]++, offset + Vector3.up * size.y + Vector3.forward, Vector3.forward, Vector2.up * size.y);
                break;
            case Facing.BACK:
                AddVertex(Indices[0]++, offset + Vector3.zero, Vector3.back, Vector2.zero);
                AddVertex(Indices[0]++, offset + Vector3.up * size.y, Vector3.back, Vector2.up * size.y);
                AddVertex(Indices[0]++, offset + Vector3.up * size.y + Vector3.right * size.x, Vector3.back, Vector2.right * size.x + Vector2.up * size.y);
                AddVertex(Indices[0]++, offset + Vector3.right * size.x, Vector3.back, Vector2.right * size.x);
                break;
            case Facing.TOP:
                AddVertex(Indices[0]++, offset + Vector3.up, Vector3.up, Vector2.zero);
                AddVertex(Indices[0]++, offset + Vector3.up + Vector3.forward * size.z, Vector3.up, Vector2.up * size.z);
                AddVertex(Indices[0]++, offset + Vector3.right * size.x + Vector3.up + Vector3.forward * size.z, Vector3.up, Vector2.right * size.x + Vector2.up * size.z);
                AddVertex(Indices[0]++, offset + Vector3.up + Vector3.right * size.x, Vector3.up, Vector2.right * size.x);
                break;
            case Facing.BOTTOM:
                AddVertex(Indices[0]++, offset + Vector3.zero, Vector3.down, Vector2.zero);
                AddVertex(Indices[0]++, offset + Vector3.right * size.x, Vector3.down, Vector3.right * size.x);
                AddVertex(Indices[0]++, offset + Vector3.right * size.x + Vector3.forward * size.z, Vector3.down, Vector2.right * size.x + Vector2.up * size.z);
                AddVertex(Indices[0]++, offset + Vector3.forward * size.z, Vector3.down, Vector2.up * size.z);
                break;
            case Facing.RIGHT:
                AddVertex(Indices[0]++, offset + Vector3.right, Vector3.right, Vector2.zero);
                AddVertex(Indices[0]++, offset + Vector3.up * size.y + Vector3.right, Vector3.right, Vector2.up * size.y);
                AddVertex(Indices[0]++, offset + Vector3.right + Vector3.up * size.y + Vector3.forward * size.z, Vector3.right, Vector2.right * size.z + Vector2.up * size.y);
                AddVertex(Indices[0]++, offset + Vector3.right + Vector3.forward * size.z, Vector3.right, Vector2.right * size.z);
                break;
            case Facing.LEFT:
                AddVertex(Indices[0]++, offset + Vector3.zero, Vector3.left, Vector2.zero);
                AddVertex(Indices[0]++, offset + Vector3.forward * size.z, Vector3.left, Vector2.right * size.z);
                AddVertex(Indices[0]++, offset + Vector3.up * size.y + Vector3.forward * size.z, Vector3.left, Vector2.right * size.z + Vector2.up * size.y);
                AddVertex(Indices[0]++, offset + Vector3.up * size.y, Vector3.left, Vector2.up * size.y);
                break;
            default:
                break;
        }
    }
}