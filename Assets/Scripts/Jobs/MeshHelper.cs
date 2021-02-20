using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


// This class contains static helper methods for use specifically with the jobs.
public static class MeshHelper
{
    // These methods are used in the main thread, so no burst optimization, but also no constraints.
    // I tried to not use this method to setup the specialized chunk perimeter arrays and instead just feed the jobs with vanilla chunk data, but the greedymeshing algorithm's performance dropped from ~86ms to ~138ms, a drastical degrade!
    // guessing the main problem is that 7 chunk datas are too large for CPU cache and there are significantly more cache misses than this compact array containing only data we need.
    public static NativeArray<uint> GetChunkWithPerimeterForJob(Dictionary<ChunkId, ChunkData> chunkDatas, ChunkId id)
    {
        var ret = new NativeArray<uint>(GameDefines.CHUNK_PERIMETER_SIZE, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        var curChunk = chunkDatas[id];
        // we first copy the entire chunk data array into the native array contiguously. This is the data we are going to loop through.
        NativeArray<uint>.Copy(curChunk.Voxels, ret, GameDefines.CHUNK_SIZE_CUBED);
        // then, for every neighbor chunk, we fill in the destination space with data. If the chunk is not loaded, we skip the face and leave the number as the default value indicating air block, in turn showing the corresponding faces without culling.
        if (chunkDatas.TryGetValue(id.Shift(Vector3Int.left), out curChunk))
        {
            // we need the right face voxels(x = CHUNK_SIZE - 1, or known as CHUNK_MASK) of the left chunk.
            var bi = GameDefines.CHUNK_SIZE_CUBED;
            for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                var outer = y * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    ret[bi + outer + z] = curChunk[GameDefines.CHUNK_MASK, y, z];
                }
            }
        }
        if (chunkDatas.TryGetValue(id.Shift(Vector3Int.right), out curChunk))
        {
            // we need the left face voxels(x = 0) of the right chunk, you get the idea.
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED;
            for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                var outer = y * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    ret[bi + outer + z] = curChunk[0, y, z];
                }
            }
        }
        // filling in the y shifted chunk datas.
        if (chunkDatas.TryGetValue(id.Shift(Vector3Int.down), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 2;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    ret[bi + outer + z] = curChunk[x, GameDefines.CHUNK_MASK, z];
                }
            }
        }
        if (chunkDatas.TryGetValue(id.Shift(Vector3Int.up), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 3;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    ret[bi + outer + z] = curChunk[x, 0, z];
                }
            }
        }
        // filling in the z shifted chunk datas.
        if (chunkDatas.TryGetValue(id.Shift(Vector3Int.back), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 4;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
                {
                    ret[bi + outer + y] = curChunk[x, y, GameDefines.CHUNK_MASK];
                }
            }
        }
        if (chunkDatas.TryGetValue(id.Shift(Vector3Int.forward), out curChunk))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 5;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
                {
                    ret[bi + outer + y] = curChunk[x, y, 0];
                }
            }
        }
        // thus, the resulting perimeter voxels are stored as follows:
        /*
         * 1. they were stored in the order left, right, down, up, back, forward.
         * 2. there's 16x16 planar voxels in each storage section.
         * 3. the other two axes are used to retrieve the corresponding voxel that has the same 2-axes coordinate.
         * 4. the coordinates are flattened in order of x, y, z. for example, if you want to retrieve data on the up face (moving along y), x and z are used to retrieve data, and corresponding coordinate is (x, z).
         * 5. I have all the methods coded in this very class, so that there is no need for anyone outside to know the details.
         */

        return ret;
    }


    // the static constants to save typing. These are used inside jobs too.
    public static readonly int4 Up = new int4(0, 1, 0, 0);
    public static readonly int4 Down = new int4(0, -1, 0, 0);
    public static readonly int4 Left = new int4(-1, 0, 0, 0);
    public static readonly int4 Right = new int4(1, 0, 0, 0);
    public static readonly int4 Forward = new int4(0, 0, 1, 0);
    public static readonly int4 Back = new int4(0, 0, -1, 0);

    // the methods to be used inside jobs. They contain the Jobs suffix, and has special optimizations.
    // 2d indices are not morton coded!
    public static int Flatten2DIndexJobs(int a, int b) => a * GameDefines.CHUNK_SIZE + b;
    public static int4 FacingToDirection(Facing f)
    {
        int4 ret = int4.zero;
        switch (f)
        {
            case Facing.RIGHT:
                ret = Right;
                break;
            case Facing.TOP:
                ret = Up;
                break;
            case Facing.BACK:
                ret = Back;
                break;
            case Facing.LEFT:
                ret = Left;
                break;
            case Facing.BOTTOM:
                ret = Down;
                break;
            case Facing.FRONT:
                ret = Forward;
                break;
        }
        return ret;
    }
    public static bool FaceIsObscuredJobs(NativeArray<uint> perimeterData, int4 index, int4 direction)
    {
        var chunkShift = ChunkData.GetChunkShift(index + direction);
        if (chunkShift.Equals(int4.zero))
        {
            return perimeterData[ChunkData.FlattenIndex(index + direction)] > 0u;
        }
        else if (chunkShift.Equals(Left))
        {
            //if we are asking for perimeter voxels on the left.
            var bi = GameDefines.CHUNK_SIZE_CUBED;
            return perimeterData[bi + Flatten2DIndexJobs(index.y, index.z)] > 0u;
        }
        else if (chunkShift.Equals(Right))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED;
            return perimeterData[bi + Flatten2DIndexJobs(index.y, index.z)] > 0u;
        }
        else if (chunkShift.Equals(Down))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 2;
            return perimeterData[bi + Flatten2DIndexJobs(index.x, index.z)] > 0u;
        }
        else if (chunkShift.Equals(Up))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 3;
            return perimeterData[bi + Flatten2DIndexJobs(index.x, index.z)] > 0u;
        }
        else if (chunkShift.Equals(Back))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 4;
            return perimeterData[bi + Flatten2DIndexJobs(index.x, index.y)] > 0u;
        }
        else if (chunkShift.Equals(Forward))
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 5;
            return perimeterData[bi + Flatten2DIndexJobs(index.x, index.y)] > 0u;
        }
        else
        {
            return false;
        }
    }
}

