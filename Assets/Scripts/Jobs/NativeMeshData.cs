﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;


public partial struct NativeMeshData
{
    public NativeArray<Vector3> Vertices;
    public NativeArray<int> Triangles;
    public NativeArray<Vector2> UVs;
    public NativeArray<int> Indices;

    public void Dispose()
    {
        Vertices.Dispose();
        Triangles.Dispose();
        UVs.Dispose();
        Indices.Dispose();
    }

    public void AddFace(Vector3 offset, Facing facing, Vector3 size)
    {
        var cp = Indices[0];
        Triangles[Indices[1]++] = cp + 0;
        Triangles[Indices[1]++] = cp + 1;
        Triangles[Indices[1]++] = cp + 2;
        Triangles[Indices[1]++] = cp + 0;
        Triangles[Indices[1]++] = cp + 2;
        Triangles[Indices[1]++] = cp + 3;

        switch (facing)
        {
            case Facing.FRONT:
                Vertices[Indices[0]++] = offset + Vector3.forward;
                Vertices[Indices[0]++] = offset + Vector3.right * size.x + Vector3.forward;
                Vertices[Indices[0]++] = offset + Vector3.right * size.x + Vector3.up * size.y + Vector3.forward;
                Vertices[Indices[0]++] = offset + Vector3.up * size.y + Vector3.forward;

                UVs[Indices[2]++] = Vector2.zero;
                UVs[Indices[2]++] = Vector2.right * size.x;
                UVs[Indices[2]++] = Vector2.right * size.x + Vector2.up * size.y;
                UVs[Indices[2]++] = Vector2.up * size.y;
                break;
            case Facing.BACK:
                Vertices[Indices[0]++] = offset + Vector3.zero;
                Vertices[Indices[0]++] = offset + Vector3.up * size.y;
                Vertices[Indices[0]++] = offset + Vector3.up * size.y + Vector3.right * size.x;
                Vertices[Indices[0]++] = offset + Vector3.right * size.x;

                UVs[Indices[2]++] = Vector2.zero;
                UVs[Indices[2]++] = Vector2.up * size.y;
                UVs[Indices[2]++] = Vector2.right * size.x + Vector2.up * size.y;
                UVs[Indices[2]++] = Vector2.right * size.x;
                break;
            case Facing.TOP:
                Vertices[Indices[0]++] = offset + Vector3.up;
                Vertices[Indices[0]++] = offset + Vector3.up + Vector3.forward * size.z;
                Vertices[Indices[0]++] = offset + Vector3.right * size.x + Vector3.up + Vector3.forward * size.z;
                Vertices[Indices[0]++] = offset + Vector3.up + Vector3.right * size.x;

                UVs[Indices[2]++] = Vector2.zero;
                UVs[Indices[2]++] = Vector2.up * size.z;
                UVs[Indices[2]++] = Vector2.right * size.x + Vector2.up * size.z;
                UVs[Indices[2]++] = Vector2.right * size.x;
                break;
            case Facing.BOTTOM:
                Vertices[Indices[0]++] = offset + Vector3.zero;
                Vertices[Indices[0]++] = offset + Vector3.right * size.x;
                Vertices[Indices[0]++] = offset + Vector3.right * size.x + Vector3.forward * size.z;
                Vertices[Indices[0]++] = offset + Vector3.forward * size.z;

                UVs[Indices[2]++] = Vector2.zero;
                UVs[Indices[2]++] = Vector2.right * size.x;
                UVs[Indices[2]++] = Vector2.right * size.x + Vector2.up * size.z;
                UVs[Indices[2]++] = Vector2.up * size.z;
                break;
            case Facing.RIGHT:
                Vertices[Indices[0]++] = offset + Vector3.right;
                Vertices[Indices[0]++] = offset + Vector3.up * size.y + Vector3.right;
                Vertices[Indices[0]++] = offset + Vector3.right + Vector3.up * size.y + Vector3.forward * size.z;
                Vertices[Indices[0]++] = offset + Vector3.right + Vector3.forward * size.z;

                UVs[Indices[2]++] = Vector2.zero;
                UVs[Indices[2]++] = Vector2.up * size.y;
                UVs[Indices[2]++] = Vector2.right * size.z + Vector2.up * size.y;
                UVs[Indices[2]++] = Vector2.right * size.z;
                break;
            case Facing.LEFT:
                Vertices[Indices[0]++] = offset + Vector3.zero;
                Vertices[Indices[0]++] = offset + Vector3.forward * size.z;
                Vertices[Indices[0]++] = offset + Vector3.up * size.y + Vector3.forward * size.z;
                Vertices[Indices[0]++] = offset + Vector3.up * size.y;

                UVs[Indices[2]++] = Vector2.zero;
                UVs[Indices[2]++] = Vector2.right * size.z;
                UVs[Indices[2]++] = Vector2.right * size.z + Vector2.up * size.y;
                UVs[Indices[2]++] = Vector2.up * size.y;
                break;
            default:
                break;
        }
    }
}