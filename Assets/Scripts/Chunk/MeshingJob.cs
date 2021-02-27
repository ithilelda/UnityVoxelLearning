using Unity.Jobs;


public class MeshingJob
{
    public ChunkId Id;
    public JobHandle Handle;
    public JobMeshing Job;
}