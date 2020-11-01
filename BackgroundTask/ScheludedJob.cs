using System;
using System.Timers;

namespace BackgroundTasks.BackgroundTask
{
    public class ScheludedJob: AbstractJob
    {
        public DateTime TargetTime { get; set; }
        public Timer Timer { get; set; }
    }
}