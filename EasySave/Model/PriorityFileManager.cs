using System;
using System.Threading;

namespace EasySave.Model
{
    public class PriorityFileManager
    {
        private int _totalJobs;
        private int _jobsDone;
        private readonly TaskCompletionSource _allPriorityDone = new TaskCompletionSource();

        public PriorityFileManager(int totalJobs)
        {
            _totalJobs = totalJobs;
            _jobsDone = 0;

            //if no jobs, signal
            if (totalJobs <= 0)
            {
                _allPriorityDone.TrySetResult();
            }
        }

        public void SignalPriorityDone()
        {
            int done = Interlocked.Increment(ref _jobsDone);
            if (done >= _totalJobs)
            {
                _allPriorityDone.TrySetResult();
            }
        }

        public Task WaitForAllPriorityAsync()
        {
            return _allPriorityDone.Task;
        }
    }
}
