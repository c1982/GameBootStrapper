using System.Threading.Tasks;
using GameBootStrapper.Unity.Runtime;
using UnityEngine;

public class BootStrapController : MonoBehaviour
{
    public TMPro.TMP_Text processText;
    
    private async void Start()
    {
        var context = new  BootStrapContext();
        var result = await BootStrapRunner.RunTasks(OnProgress, context,
            RequireComponent1, 
            RequireComponent2,
            DownloadTask1,
            DownloadTask2,
            PrepareCharacters,
            PrepareScenes,
            UploadTask,
            OpenConsentPopup
        );
        
        Debug.Log($"All tasks completed with result: {result.Success}");
    }
    
    private void OnProgress(int progress)
    {
        processText.text = $"% {progress}";
    }

    [BootStep(BootStrapTaskType.Sequential)]
    private BootStrapResult RequireComponent1(BootStrapContext ctx)
    {
        Task.Delay(1000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
    
    [BootStep(BootStrapTaskType.Sequential, suppressError: true)]
    private BootStrapResult RequireComponent2(BootStrapContext ctx)
    {
        Task.Delay(2000, ctx.ct).Wait();
        return new BootStrapResult { Success = false };
    }
    
    [BootStep(BootStrapTaskType.Parallel)]
    private BootStrapResult DownloadTask1(BootStrapContext ctx)
    {
        Task.Delay(1000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
    
    [BootStep(BootStrapTaskType.Parallel)]
    private BootStrapResult DownloadTask2(BootStrapContext ctx)
    {
        Task.Delay(2000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
    
    [BootStep(BootStrapTaskType.Parallel, 1_000)]
    private BootStrapResult PrepareCharacters(BootStrapContext ctx)
    {
        Task.Delay(3000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
    
    [BootStep(BootStrapTaskType.Parallel)]
    private BootStrapResult PrepareScenes(BootStrapContext ctx)
    {
        Task.Delay(2000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
    
    [BootStep(BootStrapTaskType.Parallel)]
    private BootStrapResult UploadTask(BootStrapContext ctx)
    {
        Task.Delay(1000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
    
    [BootStep(BootStrapTaskType.Forget)]
    private BootStrapResult OpenConsentPopup(BootStrapContext ctx)
    {
        Task.Delay(6000, ctx.ct).Wait();
        return new BootStrapResult { Success = true };
    }
}