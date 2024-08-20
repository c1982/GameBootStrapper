using System;
using System.Threading;
using System.Threading.Tasks;
using GameBootStrapper.Unity.Runtime;
using Moq;
using NUnit.Framework;

namespace GameBootStrapper.Unity.Tests.Runtime
{
    [TestFixture]
    public class BootStrapRunnerTests
    {
        private BootStrapContext _context;
        private Mock<Func<BootStrapContext, BootStrapResult>> _mockTask;
        private Action<int> _progressAction;

        [SetUp]
        public void Setup()
        {
            _context = new BootStrapContext
            {
                cancellationTokenSource = new CancellationTokenSource(),
                totalTaskCount = 1
            };
            
            _mockTask = new Mock<Func<BootStrapContext, BootStrapResult>>();
            _progressAction = _ => { };
        }

        [Test]
        public async void RunTasks_ShouldRunSequentialTaskSuccessfully()
        {
            [BootStep(BootStrapTaskType.Sequential)]
            BootStrapResult SuccessfulTask(BootStrapContext ctx) => new() { Success = true };
            
            var result = await BootStrapRunner.RunTasks(_progressAction, _context, SuccessfulTask);
            
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, _context.completedTaskCount);
        }
        
        [Test]
        public async void RunTasks_ShouldSuppressErrorAndContinue()
        {
            [BootStep(BootStrapTaskType.Sequential, suppressError: true)]
            BootStrapResult FailingTask(BootStrapContext ctx) => new() { Success = false };
            
            var result = await BootStrapRunner.RunTasks(_progressAction, _context, FailingTask);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, _context.completedTaskCount);
        }
        
        [Test]
        public async void RunTasks_ShouldTimeout()
        {
            var result = await BootStrapRunner.RunTasks(_progressAction, _context, LongRunningTask);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.EndsWith("(The operation has timed out.)"));
            Assert.AreEqual(1, _context.completedTaskCount);
            return;

            [BootStep(BootStrapTaskType.Sequential,timeOut: 1000)]
            BootStrapResult LongRunningTask(BootStrapContext ctx)
            {
                Task.Delay(2000, ctx.ct).Wait();
                return new BootStrapResult { Success = true };
            }
        }
        
        [Test]
        public async void RunTasks_ShouldCancelOnFailure()
        {
            var result = await BootStrapRunner.RunTasks(_progressAction, _context, FailingTask);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, _context.completedTaskCount);
            Assert.IsTrue(_context.ct.IsCancellationRequested);
            Assert.IsTrue(result.Message.EndsWith("(Test Exception)"));
            return;

            [BootStep(BootStrapTaskType.Parallel)]
            BootStrapResult FailingTask(BootStrapContext ctx)
            {
                throw new Exception("Test Exception");
            }
        }
        
        [Test]
        public async void RunTasks_ShouldHandleOperationCanceledException()
        {
            _context.cancellationTokenSource.Cancel();
            
            var result = await BootStrapRunner.RunTasks(_progressAction, _context, FailingTask);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, _context.completedTaskCount);
            Assert.IsTrue(_context.ct.IsCancellationRequested);
            Assert.IsTrue(result.Message.EndsWith("(A task was canceled.)"));
            
            return;
            [BootStep(BootStrapTaskType.Sequential)]
            BootStrapResult FailingTask(BootStrapContext ctx)
            {
                throw new OperationCanceledException();
            }
        }
    }
}