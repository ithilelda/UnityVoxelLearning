using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;


public class BatchBakingJob : IBatchJob
{
    public bool IsCompleted { get => _isCompleted; }

    private bool _isCompleted = false;
    private NativeArray<int> ids;
    private JobHandle handle;
    private ChunkManager manager;
    private List<ChunkId> chunkIds;

    public BatchBakingJob(Mesh[] meshes, List<ChunkId> chunkIds, ChunkManager manager)
    {
        this.manager = manager;
        this.chunkIds = chunkIds;
        var ids = new NativeArray<int>(meshes.Length, Allocator.Persistent);
        for (int i = 0; i < meshes.Length; i++)
        {
            ids[i] = meshes[i].GetInstanceID();
        }
        var job = new JobBakingCollider
        {
            MeshIds = ids
        };
        this.ids = ids;
        handle = job.Schedule(meshes.Length, 16);
    }
    public void Run()
    {
        if (handle.IsCompleted)
        {
            handle.Complete();
            ids.Dispose();
            _isCompleted = true;
        }
    }
    public IBatchJob OnCompletion()
    {
        foreach (var id in chunkIds)
        {
            manager.ChunkViews[id].SetBakedMesh();
        }
        return null;
    }
}

