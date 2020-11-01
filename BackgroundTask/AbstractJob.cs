using System;
using System.Threading.Tasks;

namespace BackgroundTasks.BackgroundTask
{
    public abstract class AbstractJob
    {
        public string Id { get; set; }
        public Task Task { get; set; }
        public string Status { get; set; } = "Initialized";
        public Exception Exception { get; set; }
    }
}