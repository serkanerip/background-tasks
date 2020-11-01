using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundTasks.BackgroundTask
{
    public class BackgroundJobStorage
    {
        private static BackgroundJobStorage _instance;
        private readonly Queue<Job> _tasks;
        private readonly List<Job> _failedJobs;
        private readonly List<ScheduledJob> _scheduledJobs;
        private readonly List<Action> _enqueuSubs;
        private readonly List<Action> _scheduleSubs;

        public List<Job> FailedJobs => _failedJobs;

        private BackgroundJobStorage ()
        {
            _scheduledJobs = new List<ScheduledJob>();
            _tasks = new Queue<Job>();
            _failedJobs = new List<Job>();
            _enqueuSubs = new List<Action>();
            _scheduleSubs = new List<Action>();
        }

        public void SubscribeToEnqueue(Action action)
        {
            _enqueuSubs.Add(action);
        }

        public List<ScheduledJob> GetScheduledJobs()
        {
            return _scheduledJobs;
        }

        public void RemoveScheduledJob(string id)
        {
            lock (_scheduledJobs)
            {
                _scheduledJobs.Remove(_scheduledJobs.Find(j => j.Id == id));    
            }
        }

        public static BackgroundJobStorage GetStorage()
        {
            return _instance ??= new BackgroundJobStorage();
        }

        public void AddToFailedJob(Job job)
        {
            _failedJobs.Add(job);
        }

        public List<Job> GetTasks()
        {
            return _tasks.ToList();
        }

        public Job GetTaskFromQueue()
        {
            return _tasks.Peek();
        }

        public bool IsEmpty()
        {
            return _tasks.Count < 1;
        }

        public Job Dequeue()
        {
            return _tasks.Dequeue();
        }

        public Job AddTask(Task t)
        {
            var j = new Job()
            {
                Id = GenerateId(),
                Task = t
            };
            Enqueu(j);
            return j;
        }

        private string GenerateId()
        {
            Guid g = Guid.NewGuid();
            var guidString = Convert.ToBase64String(g.ToByteArray());
            guidString = guidString.Replace("=","");
            guidString = guidString.Replace("+","");
            return guidString;
        }

        private void Enqueu(Job job)
        {
            _tasks.Enqueue(job);
            _enqueuSubs.ForEach(action => action.Invoke());
        }

        public void AddJob(Job job)
        {
            Enqueu(job);
        }

        public ScheduledJob AddScheduledJob(Task task, TimeSpan delay)
        {
            var job = new ScheduledJob()
            {
                Id = GenerateId(),
                Task =  task,
                TargetTime = DateTime.Now.Add(delay)
            };
            _scheduledJobs.Add(job);
            _scheduleSubs.ForEach(action => action.Invoke());
            return job;
        }

        public void SubscribeToNewScheduled(Action action)
        {
            _scheduleSubs.Add(action);
        }
    }
}