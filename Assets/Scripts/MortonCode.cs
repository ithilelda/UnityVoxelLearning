using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


// the helper class that contains all things related to calculating morton code (i.e. z-curve).
public static class MortonCode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Encode(int x, int y, int z)
    {
        return (Part1024By2(x) << 2) | (Part1024By2(y) << 1) | Part1024By2(z);
    }

    // inserting two 0s between each bit for a 10 bit number (max 1024). credit: fgiesen.wordpress.com @The ryg blog.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Part1024By2(int a)
    {
        long t = a & 0x000003ff;
        t = (t ^ (t << 16)) & 0xff0000ff;
        t = (t ^ (t << 8)) & 0x0300f00f;
        t = (t ^ (t << 4)) & 0x030c30c3;
        t = (t ^ (t << 2)) & 0x09249249;
        return (int)t;
    }
    // packing. same credit.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compact1024By2(int a)
    {
        long t = a & 0x09249249;
        t = (t ^ (t >> 2)) & 0x030c30c3;
        t = (t ^ (t >> 4)) & 0x0300f00f;
        t = (t ^ (t >> 8)) & 0xff0000ff;
        t = (t ^ (t >> 16)) & 0x000003ff;
        return (int)t;
    }
}

