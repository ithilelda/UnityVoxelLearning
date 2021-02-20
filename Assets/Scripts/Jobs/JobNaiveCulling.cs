using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;


[BurstCompile]
public struct JobNaiveCulling : IJob
{
    [ReadOnly]
    public NativeArray<uint> Data;

    public NativeMeshData MeshData;

    public void Dispose()
    {
        Data.Dispose();
        MeshData.Dispose();
    }

    public void Execute()
    {
        for (var x = 0; x < GameDefines.CHUNK_SIZE; x++)
        {
            for (var y = 0; y < GameDefines.CHUNK_SIZE; y++)
            {
                for (var z = 0; z < GameDefines.CHUNK_SIZE; z++)
                {
                    var voxelType = Data[ChunkData.FlattenIndex(x, y, z)];
                    if (voxelType > 0u)
                    {
                        var index = new int4(x, y, z, 0);
                        var pos = new Vector3(x, y, z);
                        if (!MeshHelper.FaceIsObscuredJobs(Data, index, MeshHelper.Forward))
                        {
                            MeshData.AddFace(pos, Facing.FRONT, Vector3.one);
                        }
                        if (!MeshHelper.FaceIsObscuredJobs(Data, index, MeshHelper.Back))
                        {
                            MeshData.AddFace(pos, Facing.BACK, Vector3.one);
                        }
                        if (!MeshHelper.FaceIsObscuredJobs(Data, index, MeshHelper.Up))
                        {
                            MeshData.AddFace(pos, Facing.TOP, Vector3.one);
                        }
                        if (!MeshHelper.FaceIsObscuredJobs(Data, index, MeshHelper.Down))
                        {
                            MeshData.AddFace(pos, Facing.BOTTOM, Vector3.one);
                        }
                        if (!MeshHelper.FaceIsObscuredJobs(Data, index, MeshHelper.Right))
                        {
                            MeshData.AddFace(pos, Facing.RIGHT, Vector3.one);
                        }
                        if (!MeshHelper.FaceIsObscuredJobs(Data, index, MeshHelper.Left))
                        {
                            MeshData.AddFace(pos, Facing.LEFT, Vector3.one);
                        }
                    }
                }
            }
        }
        //Debug.Log($"inside job: {MeshData.Indices[0]}, {MeshData.Indices[1]}");
    }
}
