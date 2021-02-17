using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;


public struct NativeMeshData
{
    public NativeArray<Vector3> Vertices;
    public NativeArray<int> Triangles;
    public NativeArray<int> Indices;

    public void Dispose()
    {
        Vertices.Dispose();
        Triangles.Dispose();
        Indices.Dispose();
    }

    public void AddVertex(Vector3 vertex)
    {
        if (Indices[0] >= Vertices.Length)
        {
            var tv = new NativeArray<Vector3>(Indices[0] + GameDefines.MESHGEN_ARRAY_HEADROOM, Allocator.TempJob);
            tv.CopyFrom(Vertices);
            Vertices.Dispose();
            Vertices = tv;
        }
        Vertices[Indices[0]++] = vertex;
    }

    public void AddTriangle(int trianglePoint)
    {
        if (Indices[1] >= Triangles.Length)
        {
            var tt = new NativeArray<int>(Indices[1] + GameDefines.MESHGEN_ARRAY_HEADROOM, Allocator.TempJob);
            tt.CopyFrom(Triangles);
            Triangles.Dispose();
            Triangles = tt;
        }
        Triangles[Indices[1]++] = trianglePoint;
    }
}