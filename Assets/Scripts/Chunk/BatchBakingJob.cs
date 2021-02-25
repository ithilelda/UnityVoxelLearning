using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;


public class BatchBakingJob
{
    public NativeArray<int> Ids;
    public JobHandle Handle;

    public BatchBakingJob(Mesh[] meshes)
    {
        var ids = new NativeArray<int>(meshes.Length, Allocator.Persistent);
        for (int i = 0; i < meshes.Length; i++)
        {
            ids[i] = meshes[i].GetInstanceID();
        }
        var job = new JobBakingCollider
        {
            MeshIds = ids
        };
        Ids = ids;
        Handle = job.Schedule(meshes.Length, 64);
    }
}

