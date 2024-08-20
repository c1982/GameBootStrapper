using System;
using System.Threading;
using System.Threading.Tasks;
using GameBootStrapper.Unity.Runtime;
using NUnit.Framework;

namespace GameBootStrapper.Unity.Tests.Runtime
{
    [TestFixture]
    public class GameBootStrapperTests
    {
        private BootStrapContext _context;
        private Func<BootStrapContext, BootStrapResult> _mockTask;
        private Action<int> _progressAction;    
        
        [SetUp]
        public void Setup()
        {
            _context = new BootStrapContext
            {
                cancellationTokenSource = new CancellationTokenSource(),
                totalTaskCount = 1
            };
            _mockTask = new Func<BootStrapContext, BootStrapResult>(ctx =>
            {
                Thread.Sleep(1000);
                return new BootStrapResult { Success = true };
            });
            _progressAction = _ => { };
        }

    }
}