using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundTasks.BackgroundTask
{
    public static class BackgroundJob
    {
        private static readonly BackgroundJobStorage _storage = BackgroundJobStorage.GetStorage();


        public static List<Job> FailedJobs => _storage.FailedJobs;

        public static Job AddJob(Action action)
        {
            var task = new Task(action);
            return _storage.AddTask(task);
        }
        
        public static ScheduledJob Schedule(Action action,  TimeSpan delay)
        {
            var task = new Task(action);
            return _storage.AddScheduledJob(task, delay);
        }

        public static void RetryJob(string id)
        {
            _storage.AddJob(_storage.FailedJobs.Find(j => j.Id == id));
        }
    }
}