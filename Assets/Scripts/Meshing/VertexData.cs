using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[StructLayout(LayoutKind.Sequential)]
public struct VertexData
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 UV;
}

