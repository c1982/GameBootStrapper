# GameBootStrapper

GameBootStrapper is a flexible and powerful framework designed for managing and executing bootstrapping tasks in Unity projects. It supports sequential, parallel, and fire-and-forget task execution patterns with robust error handling, timeout management, and cancellation support.

## Features

    Task Management: Execute tasks in sequential, parallel, or fire-and-forget modes.
    Timeout Handling: Automatically cancel tasks that exceed a specified timeout.
    Error Handling: Suppress errors or stop execution on failure.
    Progress Reporting: Track and report progress of tasks.
    Cancellation Support: Gracefully cancel tasks using cancellation tokens.

## Installation

    Clone the repository or download the latest release from GitHub.
    Copy the GameBootStrapper folder into your Unity project's Assets directory.

or

    Install the package from the Unity Package Manager using the Git URL: 
    https://github.com/c1982/GameBootStrapper.git?path=Boot/Assets/Scripts/GameBootStrapper.Unity

## Getting Started

### 1. Define Your Bootstrap Tasks

Create your bootstrapping tasks by defining methods that take a BootStrapContext as a parameter and return a BootStrapResult.

```csharp
using GameBootStrapper.Unity.Runtime;

public class MyGameBootstrap
{
    [BootStep(BootStrapTaskType.Sequential, 5000)] // 5-second timeout
    public BootStrapResult LoadGameData(BootStrapContext ctx)
    {
        // Load game data logic
        return new BootStrapResult { Success = true };
    }

    [BootStep(BootStrapTaskType.Parallel)]
    public BootStrapResult InitializeAudio(BootStrapContext ctx)
    {
        // Initialize audio system
        return new BootStrapResult { Success = true };
    }

    [BootStep(BootStrapTaskType.Forget)]
    public BootStrapResult StartBackgroundMusic(BootStrapContext ctx)
    {
        // Start playing background music without waiting for completion
        return new BootStrapResult { Success = true };
    }
}
```

### 2. Create and Configure BootStrapContext

The BootStrapContext manages the state of the bootstrapping process, including cancellation and progress tracking.

```csharp
var context = new BootStrapContext
{
    cancellationTokenSource = new CancellationTokenSource(),
};
```

### 3. Run Bootstrapping Tasks

Use the BootStrapRunner.RunTasks method to execute your bootstrapping tasks. The method returns a BootStrapResult indicating the overall success or failure of the bootstrapping process.

```csharp
using System.Threading.Tasks;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private async void Start()
    {
        var context = new BootStrapContext
        {
            cancellationTokenSource = new CancellationTokenSource()
        };

        var bootstrap = new MyGameBootstrap();
        var result = await BootStrapRunner.RunTasks(UpdateProgress, context, 
            bootstrap.LoadGameData, 
            bootstrap.InitializeAudio, 
            bootstrap.StartBackgroundMusic);

        if (result.Success)
        {
            Debug.Log("Game initialized successfully!");
        }
        else
        {
            Debug.LogError($"Game initialization failed: {result.Message}");
        }
    }

    private void UpdateProgress(int percentage)
    {
        Debug.Log($"Initialization progress: {percentage}%");
    }
}
```

### 4. Handling Timeouts and Errors

Tasks can be configured to timeout or suppress errors using the [BootStep] attribute. By default, if a task fails, the remaining tasks are canceled. You can change this behavior by setting SuppressError = true.

```csharp
[BootStep(BootStrapTaskType.Sequential, 5000, SuppressError = true)]
public BootStrapResult LoadOptionalData(BootStrapContext ctx)
{
    // This task can fail without affecting other tasks
    return new BootStrapResult { Success = false, Message = "Data load failed" };
}
```

### 5. Advanced Usage

   Handling Cancellations

You can cancel the bootstrapping process programmatically by calling context.cancellationTokenSource.Cancel().

```csharp
context.cancellationTokenSource.Cancel(); // Cancel all ongoing tasks
```

Custom Failure Handling

Custom failure logic can be implemented by passing an Action to handle specific failures.

```csharp
[BootStep(BootStrapTaskType.Sequential, 10000, SuppressError = false)]
public BootStrapResult InitializeCriticalSystem(BootStrapContext ctx)
{
    try
    {
        // Critical initialization logic
    }
    catch (Exception ex)
    {
        Debug.LogError("Critical system initialization failed.");
        return new BootStrapResult { Success = false, Message = ex.Message };
    }
}
```

Contributing

Contributions are welcome! Please fork the repository and submit a pull request. For major changes, open an issue first to discuss what you would like to change.
License

This project is licensed under the MIT License - see the LICENSE file for details.
Acknowledgments

    Unity - The game engine used in this project.
    NUnit - The testing framework for unit tests.
    Moq - Used for mocking in tests.

Contact

For questions or support, please contact aspsrc@gmail.com  or open an issue on GitHub.