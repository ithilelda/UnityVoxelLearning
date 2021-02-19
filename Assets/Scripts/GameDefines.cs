using System;
using UnityEngine;

public static class GameDefines
{
    public const int CHUNK_BIT = 4;
    public const int CHUNK_SIZE = 1 << CHUNK_BIT;
    public const int CHUNK_MASK = CHUNK_SIZE - 1;
    public const int CHUNK_SIZE_SQUARED = CHUNK_SIZE * CHUNK_SIZE;
    public const int CHUNK_SIZE_CUBED = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
    public const int CHUNK_PERIMETER_SIZE = CHUNK_SIZE_CUBED + CHUNK_SIZE_SQUARED * 6;

    public const int MAXIMUM_VERTEX_ARRAY_COUNT = CHUNK_SIZE_CUBED * 4; // we set the initial vertex array size to the absolute maximum. (checkerboard pattern, 8 vertices each block.)
    public const int MAXIMUM_TRIANGLE_ARRAY_COUNT = CHUNK_SIZE_CUBED * 18; // we set the initial triangle array size to the absolute maximum. (checkerboard pattern, 12 triangles each block.)
    public const int MESHGEN_ARRAY_HEADROOM = CHUNK_SIZE_CUBED / 16; // the head room that we increase for each array resize.

    public static readonly Vector3[] CubeVertices = new[] {
        Vector3.zero,
        Vector3.up,
        Vector3.up + Vector3.right,
        Vector3.right,
        Vector3.forward,
        Vector3.up + Vector3.forward,
        Vector3.one,
        Vector3.right + Vector3.forward,
    };
}
