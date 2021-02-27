using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;


public class BatchMeshingJob
{
    public Mesh.MeshDataArray Array;
    public bool IsCompleted = false;
    public List<ChunkId> Ids = new List<ChunkId>();

    private readonly Queue<MeshingJob> firstJobs = new Queue<MeshingJob>();
    private readonly Queue<MeshingJob> secondJobs = new Queue<MeshingJob>();
    private bool isFirst;

    public BatchMeshingJob(Queue<ChunkId> ids, Dictionary<ChunkId, ChunkData> chunkDatas, int meshingType)
    {
        Array = Mesh.AllocateWritableMeshData(ids.Count);
        var index = 0;
        while (ids.Count > 0)
        {
            var id = ids.Dequeue();
            Ids.Add(id);
            var mesh = new NativeMeshData
            {
                Vertices = new NativeArray<VertexData>(GameDefines.MAXIMUM_VERTEX_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Triangles = new NativeArray<uint>(GameDefines.MAXIMUM_TRIANGLE_ARRAY_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                Indices = new NativeArray<int>(2, Allocator.TempJob)
            };
            var data = new NativeArray<uint>(GameDefines.CHUNK_SIZE_CUBED, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            data.CopyFrom(chunkDatas[id].Voxels);
            var job = new JobMeshing
            {
                Data = data,
                MeshingType = meshingType,
                MeshData = mesh
            };
            var apply = new JobApplyMesh
            {
                Data = mesh,
                MeshData = Array[index]
            };
            firstJobs.Enqueue(new MeshingJob { Id = id, Job = job, Handle = apply.Schedule(job.Schedule()) });
            index++;
        }
    }

    public void HandleJobCompletion()
    {
        var curJobs = isFirst ? firstJobs : secondJobs;
        var nextJobs = isFirst ? secondJobs : firstJobs;
        while (curJobs.Count > 0)
        {
            var chunkJob = curJobs.Dequeue();
            if (chunkJob.Handle.IsCompleted)
            {
                chunkJob.Handle.Complete();
                chunkJob.Job.Dispose();
            }
            else
            {
                nextJobs.Enqueue(chunkJob);
            }
        }
        if (firstJobs.Count == 0 && secondJobs.Count == 0)
        {
            IsCompleted = true;
        }
        isFirst = !isFirst;
    }
}

