using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;


[BurstCompile]
public struct JobBakingCollider : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<int> MeshIds;

    public void Execute(int index)
    {
        Physics.BakeMesh(MeshIds[index], false);
    }
}

