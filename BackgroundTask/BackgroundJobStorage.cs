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
        private readonly List<ScheludedJob> _scheludedJobs;
        private readonly List<Action> _enqueuSubs;
        private readonly List<Action> _scheludeSubs;

        public List<Job> FailedJobs => _failedJobs;

        private BackgroundJobStorage ()
        {
            _scheludedJobs = new List<ScheludedJob>();
            _tasks = new Queue<Job>();
            _failedJobs = new List<Job>();
            _enqueuSubs = new List<Action>();
            _scheludeSubs = new List<Action>();
        }

        public void SubscribeToEnqueue(Action action)
        {
            _enqueuSubs.Add(action);
        }

        public List<ScheludedJob> GetScheludedJobs()
        {
            return _scheludedJobs;
        }

        public void RemoveScheludedJob(string id)
        {
            lock (_scheludedJobs)
            {
                _scheludedJobs.Remove(_scheludedJobs.Find(j => j.Id == id));    
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

        public ScheludedJob AddScheludedJob(Task task, TimeSpan delay)
        {
            var job = new ScheludedJob()
            {
                Id = GenerateId(),
                Task =  task,
                TargetTime = DateTime.Now.Add(delay)
            };
            _scheludedJobs.Add(job);
            _scheludeSubs.ForEach(action => action.Invoke());
            return job;
        }

        public void SubscribeToNewScheluder(Action action)
        {
            _scheludeSubs.Add(action);
        }
    }
}