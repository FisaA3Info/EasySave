using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasySave.Model
{
    public class PriorityFileManager
    {
        private int _totalJobs;
        private int _jobsDone;
        private readonly ManualResetEventSlim _allPriorityDone = new ManualResetEventSlim(false);

        public PriorityFileManager(int totalJobs)
        {
            _totalJobs = totalJobs;
            _jobsDone = 0;

            //if no jobs, signal
            if (totalJobs <= 0)
            {
                _allPriorityDone.Set();
            }
        }

        public void SignalPriorityDone()
        {
            int done = Interlocked.Increment(ref _jobsDone);
            if (done >= _totalJobs)
            {
                _allPriorityDone.Set();
            }
        }

        public void WaitForAllPriority()
        {
            _allPriorityDone.Wait();
        }
    }
}
