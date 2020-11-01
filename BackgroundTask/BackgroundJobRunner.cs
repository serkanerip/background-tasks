using System;
using System.Threading.Tasks;

namespace BackgroundTasks.BackgroundTask
{
    public class BackgroundJobRunner: IDisposable
    {
        private readonly BackgroundJobStorage _storage = BackgroundJobStorage.GetStorage();
        private bool _isDisposed;

        public BackgroundJobRunner()
        {
            _storage.SubscribeToEnqueue(RunImmediateJobs);
            _storage.SubscribeToNewScheluder(SetTimersForScheludedJobs);
            RunImmediateJobs();
            SetTimersForScheludedJobs();
        }

        private void RunImmediateJobs()
        {
            while (!_storage.IsEmpty())
            {
                RunJob(_storage.Dequeue());
            }
        }

        private void SetTimersForScheludedJobs()
        {
            var jobs = _storage.GetScheludedJobs();
            jobs.FindAll(job => job.Timer == null).ForEach(job =>
            {
                var millis = (job.TargetTime - DateTime.Now).TotalMilliseconds;
                var timer = new System.Timers.Timer(millis) {AutoReset = false};
                timer.Elapsed += (sender, args) =>
                {
                    job.Status = "Started";
                    _storage.RemoveScheludedJob(job.Id);
                    try
                    {
                        job.Task.Start();
                    }
                    catch (Exception ex)
                    {
                        job.Status = "Failed";
                        job.Exception = job.Task.Exception;
                        _storage.AddToFailedJob(new Job()
                        {
                            Id = job.Id,
                            Task = job.Task
                        });
                    }
                    job.Task.Wait();
                    job.Status = "Completed";
                };
                timer.Enabled = true;
                job.Timer = timer;
            });
            
        }

        private void HandleScheludedJobs()
        {
            var jobs = _storage.GetScheludedJobs();
            if (jobs.Count < 1) return;
            jobs.ForEach(job =>
            {
                if (job.TargetTime > DateTime.Now) return;
                Task.Run(() => RunJob(new Job()
                {
                    Id = job.Id,
                    Task = job.Task
                }));
                _storage.RemoveScheludedJob(job.Id);
            });
        }

        private void RunJob(Job job)
        {
            job.Status = "Started";
            try
            {
                job.Task.Start();
            }
            catch (Exception ex)
            {
                // ignored
            }

            job.Task.GetAwaiter().OnCompleted(() =>
            {
                if (job.Task.Exception != null)
                {
                    job.Status = "Failed";
                    job.Exception = job.Task.Exception;
                    _storage.AddToFailedJob(job);
                }
                else
                {
                    job.Status = "Completed";
                }
            });
        }

        public void Dispose()
        {
            Console.WriteLine("Disposed");
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}