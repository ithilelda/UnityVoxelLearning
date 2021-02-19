using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkId : IEquatable<ChunkId>
{
    public readonly int x;
    public readonly int y;
    public readonly int z;

    public ChunkId(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public ChunkId Shift(Vector3Int shift)
    {
        return new ChunkId(x + shift.x, y + shift.y, z + shift.z);
    }

    public bool Equals(ChunkId other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is ChunkId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return MortonCode.Encode(x, y, z);
    }

    public static bool operator ==(ChunkId left, ChunkId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChunkId left, ChunkId right)
    {
        return !left.Equals(right);
    }
}