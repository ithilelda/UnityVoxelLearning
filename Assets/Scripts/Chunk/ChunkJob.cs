using Unity.Jobs;


public class ChunkJob
{
    public ChunkId Id;
    public JobHandle Handle;
    public IJob Job;
}

