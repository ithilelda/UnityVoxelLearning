using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IBatchJob
{
    public bool IsCompleted { get; }
    public void Run();
    public IBatchJob OnCompletion();
}

