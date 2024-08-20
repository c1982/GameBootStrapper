using System.Threading;

namespace GameBootStrapper.Unity.Runtime
{
    public class BootStrapContext
    { 
        public CancellationTokenSource cancellationTokenSource { get; set; } = new();
        public CancellationToken ct => cancellationTokenSource.Token; 
        public int totalTaskCount { get; set; } 
        public int completedTaskCount { get; private set; }
    
        public void IncrementCompletedTaskCount()
        {
            lock (this)
            {
                completedTaskCount++;
            }
        }
    }
}