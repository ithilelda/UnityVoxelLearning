using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;


public class BatchMeshingJob
{
    public ChunkManager Manager;
    public Queue<MeshingJob> Jobs = new Queue<MeshingJob>();
    public Queue<ChunkId> Ids = new Queue<ChunkId>();
    public Mesh.MeshDataArray Array;
    public Mesh[] Meshes;
    public bool IsCompleted;

    public BatchMeshingJob(int num)
    {
        Meshes = new Mesh[num];
    }

    public void HandleJobCompletion()
    {
        var start = Time.realtimeSinceStartup;
        var elapsed = 0f;
        while (Jobs.Count > 0 && elapsed <= 0.01f)
        {
            var chunkJob = Jobs.Dequeue();
            if (chunkJob.Handle.IsCompleted)
            {
                chunkJob.Handle.Complete();
                if (Manager.MeshingType == 1)
                {
                    var job = (JobNaiveCulling)chunkJob.Job;
                    job.Dispose();
                }
                else if (Manager.MeshingType == 2)
                {
                    var job = (JobGreedyMeshing)chunkJob.Job;
                    job.Dispose();
                }
            }
            else
            {
                Jobs.Enqueue(chunkJob);
            }
            elapsed = Time.realtimeSinceStartup - start;
        }
        if (Jobs.Count == 0)
        {
            Mesh.ApplyAndDisposeWritableMeshData(Array, Meshes);
            IsCompleted = true;
        }
    }
}

