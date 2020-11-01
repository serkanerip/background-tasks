using System;
using System.Timers;

namespace BackgroundTasks.BackgroundTask
{
    public class ScheduledJob: AbstractJob
    {
        public DateTime TargetTime { get; set; }
        public Timer Timer { get; set; }
    }
}