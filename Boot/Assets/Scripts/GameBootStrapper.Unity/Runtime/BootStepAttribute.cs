using System;

namespace GameBootStrapper.Unity.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BootStepAttribute : Attribute
    {
        public BootStrapTaskType TaskType { get; set; }
        public TimeSpan Timeout { get; set; }
    
        public bool SuppressError { get; set; }
    
        public BootStepAttribute(BootStrapTaskType type, int timeOut = 25_000, bool suppressError = false)
        {
            TaskType = type;
            Timeout = TimeSpan.FromMilliseconds(timeOut);
            SuppressError = suppressError;
        }
    }
}