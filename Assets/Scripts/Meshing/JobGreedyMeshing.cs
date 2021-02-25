using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;


[BurstCompile]
public struct JobGreedyMeshing : IJob
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
        // what 0~6 means you should check out the Facing enum. It has explanations.
        for (int face = 0; face < 6; face++)
        {
            var direction = MeshHelper.FacingToDirection((Facing)face); // the offset vector used to check the visibility of faces in MeshHelper.FaceIsObscuredJobs method.
            var slicingAxis = face % 3; // the current axis we are working on. 0 -> x, 1 -> y, 2 -> z.
            // if we are working on the x axis, the plane is YZ. if we are working on the y axis, the plane is XZ, and so on so forth.
            var axis1 = (slicingAxis + 1) % 3;
            var axis2 = (slicingAxis + 2) % 3;
            var startPos = new int4();
            // we slice the chunk data along the current axis we are working on.
            for (startPos[slicingAxis] = 0; startPos[slicingAxis] < GameDefines.CHUNK_SIZE; startPos[slicingAxis]++)
            {
                // this is the boolean array that tells us if the voxel has already been merged or not, so we can correctly skip it in the loop.
                var merged = new NativeArray<bool>(GameDefines.CHUNK_SIZE_SQUARED, Allocator.Temp);
                // for each slice, we need to loop over the 16x16 voxels on that plane.
                for (startPos[axis1] = 0; startPos[axis1] < GameDefines.CHUNK_SIZE; startPos[axis1]++)
                {
                    for (startPos[axis2] = 0; startPos[axis2] < GameDefines.CHUNK_SIZE; startPos[axis2]++)
                    {
                        // this voxel at this position is now the candidate for a merge, so we call it the start.
                        var startVoxel = Data[ChunkData.FlattenIndex(startPos)];
                        var startIndex = MeshHelper.Flatten2DIndexJobs(startPos[axis1], startPos[axis2]);
                        // if the voxel has been merged, or it is air, or the face we are working on is obscured, we skip it.
                        if (merged[startIndex] || startVoxel == 0u || MeshHelper.FaceIsObscuredJobs(Data, startPos, direction))
                        {
                            continue;
                        }
                        // if we can actually merge, we start merging now.
                        // first, we reset the quadSize vector. This vector stores the size info of our quad.
                        var quadSize = new Vector3Int();
                        // Now, we check along the axis2 axis for merges. We start with the next voxel, so we set curPos to startPos with axis2 increased.
                        var curPos = startPos;
                        curPos[axis2]++;
                        // we move along the axis2 (minor axis) to check if the current voxel is mergeable:
                        /*
                         * 1. we stop if the position is outside the index boundary.
                         * 2. the current voxel must not already be merged.
                         * 3. the current voxel can be merged with the start voxel.
                        */
                        while (curPos[axis2] < GameDefines.CHUNK_SIZE
                            && !merged[MeshHelper.Flatten2DIndexJobs(curPos[axis1], curPos[axis2])]
                            && IsMergeable(curPos, startVoxel, direction))
                        {
                            curPos[axis2]++;
                        }
                        // then we store the row size into our quadSize variable.
                        quadSize[axis2] = curPos[axis2] - startPos[axis2];
                        // then we expand along the axis1 (major axis) to check for another dimension. we start with next voxel as usual.
                        curPos = startPos;
                        curPos[axis1]++;
                        while (curPos[axis1] < GameDefines.CHUNK_SIZE
                            && !merged[MeshHelper.Flatten2DIndexJobs(curPos[axis1], curPos[axis2])]
                            && IsMergeable(curPos, startVoxel, direction))
                        {
                            // if the first voxel passes the check, we can start from the second right away.
                            curPos[axis2]++;
                            // checking along the minor axis.
                            while (curPos[axis2] < GameDefines.CHUNK_SIZE
                                && !merged[MeshHelper.Flatten2DIndexJobs(curPos[axis1], curPos[axis2])]
                                && IsMergeable(curPos, startVoxel, direction))
                            {
                                curPos[axis2]++;
                            }
                            // however, if we failed the check before reaching the length of the first row, then this row is bad, we discard the row, and end the merge.
                            if (curPos[axis2] - startPos[axis2] < quadSize[axis2])
                            {
                                break;
                            }
                            // otherwise, if we reached the quadSize, then this row is good. we reset the axis2 counter to our start position, and start the next row.
                            else
                            {
                                curPos[axis2] = startPos[axis2];
                            }
                            curPos[axis1]++;
                        }
                        // after we successfully check the major axis, we store the column size into quadSize.
                        quadSize[axis1] = curPos[axis1] - startPos[axis1];

                        // then we add the quad to our mesh.
                        MeshData.AddFace(new Vector3(startPos.x, startPos.y, startPos.z), (Facing)face, quadSize);

                        // finally we mark the voxels that make up the quad merged.
                        for (int i = 0; i < quadSize[axis1]; i++)
                        {
                            for (int j = 0; j < quadSize[axis2]; j++)
                            {
                                var q = MeshHelper.Flatten2DIndexJobs(i, j);
                                merged[startIndex + q] = true;
                            }
                        }
                    }
                }
                // don't forget to dispose the native array.
                merged.Dispose();
            }
        }
    }
    /* checking if the voxel at curPos can be merged with the targetVoxel.
     * 1. the current voxel must be a solid voxel.
     * 2. the current voxel is the same type as the target voxel.
     * 3. the current voxel's face at direction must be visible.
     */
    public bool IsMergeable(int4 curPos, uint targetVoxel, int4 direction)
    {
        var curVoxel = Data[ChunkData.FlattenIndex(curPos)];
        return curVoxel != 0u && curVoxel == targetVoxel && !MeshHelper.FaceIsObscuredJobs(Data, curPos, direction);
    }
}

