using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GameBootStrapper.Unity.Runtime
{
    public static class BootStrapRunner
    {
        public static async Task<BootStrapResult> RunTasks(Action<int> progress, BootStrapContext ctx, 
            params Func<BootStrapContext, BootStrapResult>[] tasks)
        {
            ctx.totalTaskCount = tasks.Length;
            var parallelTasks = new List<Task<BootStrapResult>>();
            foreach (var fn in tasks)
            {
                var meta = fn.Method.GetCustomAttribute<BootStepAttribute>();
                if(meta == null)
                    continue;

                switch (meta.TaskType)
                {
                    case BootStrapTaskType.Sequential:
                        var result = await ExecuteTaskAsync(fn, ctx, progress, meta);
                        if (!result.Success)
                            return result;
                        break;
                    case BootStrapTaskType.Parallel:
                        parallelTasks.Add(ExecuteTaskAsync(fn, ctx, progress, meta));
                        break;
                    case BootStrapTaskType.Forget:
                        _ = ExecuteTaskAsync(fn, ctx, progress, meta);
                        break;
                }
            }

            if (!parallelTasks.Any()) 
                return new BootStrapResult() { Success = true };
            
            var parallelTaskResults = await Task.WhenAll(parallelTasks);
            var failedTaskResult = parallelTaskResults.FirstOrDefault(r => !r.Success);
            
            return failedTaskResult ?? new BootStrapResult { Success = true };
        }

        private static async Task<BootStrapResult> ExecuteTaskAsync(Func<BootStrapContext, BootStrapResult> task, 
            BootStrapContext ctx, Action<int> progress, BootStepAttribute meta)
        {
            try
            {
                var result = await Task.Run(() => Task.FromResult(task(ctx)), cancellationToken: ctx.ct)
                    .TimeOut(meta.Timeout, ctx.ct);
                
                ctx.IncrementCompletedTaskCount();
                progress?.Invoke(CalculateProgressPercentage(ctx));
                Debug.Log($"Step {task.Method.Name} completed with result: {result.Success}");
                
                if(meta.SuppressError)
                    return new BootStrapResult(){ Success = true , Message = "Suppressed error"};
                
                if (!result.Success)
                    ctx.cancellationTokenSource.Cancel();
                
                return result;
            }
            catch (Exception ex)
            {
                if(!ctx.ct.IsCancellationRequested)
                    ctx.cancellationTokenSource.Cancel();
                
                Debug.LogWarning($"Step {task.Method.Name} failed with error: {ex.Message}");
                return new BootStrapResult { Success = false, Message = ex.Message };
            }
        }
        
        private static int CalculateProgressPercentage(BootStrapContext ctx)
        {
            return (int)((float)ctx.completedTaskCount / ctx.totalTaskCount * 100);
        }

        private static async Task<TResult> TimeOut<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            if (completedTask == timeoutTask)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                throw new TimeoutException("The operation has timed out.");
            }
            
            return await task;
        }
    }
}