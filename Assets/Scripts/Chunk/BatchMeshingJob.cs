using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;


public class BatchMeshingJob : IBatchJob
{
    public bool IsCompleted { get => _isCompleted; }

    private bool _isCompleted = false;
    private readonly DoubleQueue<MeshingJob> jobs = new DoubleQueue<MeshingJob>();
    private ChunkManager manager;
    private Mesh.MeshDataArray dataArray;
    private List<ChunkId> ids = new List<ChunkId>();

    public BatchMeshingJob(IEnumerable<ChunkId> ids, int meshingType, ChunkManager manager)
    {
        this.manager = manager;
        dataArray = Mesh.AllocateWritableMeshData(ids.Count());
        var index = 0;
        foreach (var id in ids)
        {
            this.ids.Add(id);
            var mesh = new NativeMeshData
            {
                Vertices = new NativeArray<VertexData>(GameDefines.MAXIMUM_VERTEX_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Triangles = new NativeArray<uint>(GameDefines.MAXIMUM_TRIANGLE_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Indices = new NativeArray<int>(2, Allocator.TempJob)
            };
            var data = new NativeArray<uint>(GameDefines.CHUNK_SIZE_CUBED, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            data.CopyFrom(manager.ChunkDatas[id].Voxels);
            var job = new JobMeshing
            {
                InputData = data,
                MeshingType = meshingType,
                OutputData = mesh
            };
            var apply = new JobApplyMesh
            {
                InputData = mesh,
                OutputData = dataArray[index]
            };
            jobs.Enqueue(new MeshingJob { Id = id, Job = job, Handle = apply.Schedule(job.Schedule()) });
            index++;
        }
    }

    public void Run()
    {
        while (jobs.CurrentCount > 0)
        {
            var chunkJob = jobs.Dequeue();
            if (chunkJob.Handle.IsCompleted)
            {
                chunkJob.Handle.Complete();
                chunkJob.Job.Dispose();
            }
            else
            {
                jobs.Enqueue(chunkJob);
            }
        }
        if (jobs.Count == 0)
        {
            _isCompleted = true;
        }
        jobs.Swap();
    }
    public IBatchJob OnCompletion()
    {
        var meshes = new Mesh[ids.Count];
        for (int i = 0; i < ids.Count; i++)
        {
            meshes[i] = manager.ChunkViews[ids[i]].GetMesh();
        }
        Mesh.ApplyAndDisposeWritableMeshData(dataArray, meshes);
        return new BatchBakingJob(meshes, ids, manager);
    }
}

