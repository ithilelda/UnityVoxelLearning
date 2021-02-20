using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;


public struct JobPerimeterChunkData : IJob
{
    //input.
    [ReadOnly]
    public NativeArray<uint> Current;
    [ReadOnly]
    public NativeArray<uint> Left;
    [ReadOnly]
    public NativeArray<uint> Right;
    [ReadOnly]
    public NativeArray<uint> Top;
    [ReadOnly]
    public NativeArray<uint> Bottom;
    [ReadOnly]
    public NativeArray<uint> Front;
    [ReadOnly]
    public NativeArray<uint> Back;
    public bool HasLeft, HasRight, HasTop, HasBottom, HasFront, HasBack;

    //output.
    public NativeArray<uint> Output;

    public void Execute()
    {
        // we first copy the current chunk data array into the output array contiguously. This is the data we are going to loop through so we preserve order.
        NativeArray<uint>.Copy(Current, Output, GameDefines.CHUNK_SIZE_CUBED);
        // then, for every neighbor chunk, we fill in the destination space with data. If the chunk is not loaded, we skip the face and leave the number as the default value indicating air block, in turn showing the corresponding faces without culling.
        if (HasLeft)
        {
            // we need the right face voxels(x = CHUNK_SIZE - 1, or known as CHUNK_MASK) of the left chunk.
            var bi = GameDefines.CHUNK_SIZE_CUBED;
            for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                var outer = y * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    Output[bi + outer + z] = Left[ChunkData.FlattenIndex(GameDefines.CHUNK_MASK, y, z)];
                }
            }
        }
        if (HasRight)
        {
            // we need the left face voxels(x = 0) of the right chunk, you get the idea.
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED;
            for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                var outer = y * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    Output[bi + outer + z] = Right[ChunkData.FlattenIndex(0, y, z)];
                }
            }
        }
        // filling in the y shifted chunk datas.
        if (HasBottom)
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 2;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    Output[bi + outer + z] = Bottom[ChunkData.FlattenIndex(x, GameDefines.CHUNK_MASK, z)];
                }
            }
        }
        if (HasTop)
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 3;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    Output[bi + outer + z] = Top[ChunkData.FlattenIndex(x, 0, z)];
                }
            }
        }
        // filling in the z shifted chunk datas.
        if (HasBack)
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 4;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
                {
                    Output[bi + outer + y] = Back[ChunkData.FlattenIndex(x, y, GameDefines.CHUNK_MASK)];
                }
            }
        }
        if (HasFront)
        {
            var bi = GameDefines.CHUNK_SIZE_CUBED + GameDefines.CHUNK_SIZE_SQUARED * 5;
            for (int x = 0; x < GameDefines.CHUNK_SIZE; x++)
            {
                var outer = x * GameDefines.CHUNK_SIZE;
                for (int y = 0; y < GameDefines.CHUNK_SIZE; y++)
                {
                    Output[bi + outer + y] = Front[ChunkData.FlattenIndex(x, y, 0)];
                }
            }
        }
    }
}

