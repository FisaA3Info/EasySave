using EasySave.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using EasyLog;

namespace EasySave.ViewModel
{
    //manages the taks/jobs 
    internal class BackupManager
    {
        //=================  attributes ====================
        private Logger logger;
        private StateTracker stateTracker;
        private const int MAX_JOBS = 5;
        public List<BackupJob> BackupJobs { get; set; }


        //================ Contsructor  ===================
        public BackupManager(Logger logger = null, StateTracker stateTracker = null)
        {
            this.logger = logger;
            this.stateTracker = stateTracker;
            BackupJobs = new List<BackupJob>();
        }


        //================ Methods  =======================
        //Use try catch for the error management (Maybe Error class ?)
        public bool CreateJob(string jobName, string sourcePath, string destinationPath, BackupType type)
        {

            if (string.IsNullOrWhiteSpace(jobName) || string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
            {
                //logger?.Log("CreateJob failed: invalid parameters.");
                return false;
            }

            if (BackupJobs.Count >= MAX_JOBS)
            {
                //logger?.Log($"CreateJob failed: max jobs ({MAX_JOBS}) reached.");
                return false;
            }
            //uses the managejob constructor if less than 5 jobs
            try
            {
                var newJob = new BackupJob(jobName, sourcePath, destinationPath, type);

                BackupJobs.Add(newJob);
                //logger?.Log($"Job created: {jobName}");
                stateTracker?.UpdateState(new StateEntry(newJob.Name ?? jobName, BackupState.Inactive));
                return true;
            }
            catch (Exception e)
            {
                //logger?.Log($"CreateJob error: {e.Message}");
                return false;
            }
        }

        //Deletes an obkect based on the index
        public bool DeleteJob(int index)
        {
            if (index < 1 || index > BackupJobs.Count)
            {
                //logger?.Log($"DeleteJob failed: invalid index {index}.");
                return false;
            }

            try
            {
                //deletes the job object and removes it from the list²
                var removed = BackupJobs[index - 1];
                BackupJobs.RemoveAt(index - 1);
                //logger?.Log($"Job deleted: {removed?.Name ?? $"#{index}"}");
                stateTracker?.RemoveState(removed?.Name ?? string.Empty);
                return true;
            }
            catch (Exception e)
            {
                //logger?.Log($"DeleteJob error: {e.Message}");
                return false;
            }
        }

        //uses the managejob method to execute itself
        public bool ExecuteJob(int index)
        {

            if (index < 1 || index > BackupJobs.Count)
            {
                //logger?.Log($"ExecuteJob failed: invalid index {index}.");
                return false;
            }

            var job = BackupJobs[index - 1];
            try
            {
                //logger?.Log($"Starting job #{index}: {job?.Name}");
                job.Execute(logger, stateTracker);
                var entry = new StateEntry(job.Name ?? string.Empty, job.State ?? BackupState.Inactive);
                stateTracker?.UpdateState(entry);
                //logger?.Log($"Finished job #{index}: {job?.Name}");
                return true;
            }
            catch (Exception e)
            {
                //logger?.Log($"ExecuteJob error for #{index}: {e}");
                var errEntry = new StateEntry(job.Name ?? string.Empty, BackupState.OnError);
                return false;
            }
        }

        //executes all the jobs in the list
        public void ExecuteAllJobs()
        {
            //logger?.Log("ExecuteAllJobs: starting.");
            for (int i = 1; i <= BackupJobs.Count; i++)
            {
                var ok = ExecuteJob(i);
                if (!ok)
                {
                    //logger?.Log($"ExecuteAllJobs: stopped at #{i} due to error.");
                    break;
                }
            }
            //logger?.Log("ExecuteAllJobs: finished.");
        }

        //executes a range of jobs
        public void ExecuteRange(int start, int end)
        {
            if (BackupJobs.Count == 0)
            {
                //logger?.Log("ExecuteRange: no jobs to execute.");
                return;
            }

            if (start > end) (start, end) = (end, start);

            start = Math.Max(1, start);
            end = Math.Min(BackupJobs.Count, end);

            //logger?.Log($"ExecuteRange: executing jobs {start} to {end}.");
            for (int i = start; i <= end; i++)
            {
                if (!ExecuteJob(i))
                {
                    //logger?.Log($"ExecuteRange: stopped at job #{i} due to error.");
                    break;
                }
            }
            //logger?.Log("ExecuteRange: finished.");
        }

        //executes specific jobs by its index
        public void ExecuteSelection(List<int> indexes)
        {
            if (indexes == null || indexes.Count == 0)
            {
                //logger?.Log("ExecuteSelection: empty selection.");
                return;
            }

            var normalized = indexes
                .Where(i => i >= 1 && i <= BackupJobs.Count)
                .Distinct()
                .OrderBy(i => i);

            if (!normalized.Any())
            {
                //logger?.Log("ExecuteSelection: no valid indices.");
                return;
            }

            //logger?.Log($"ExecuteSelection: executing jobs {string.Join(",", normalized)}.");
            foreach (var idx in normalized)
            {
                if (!ExecuteJob(idx))
                {
                    //logger?.Log($"ExecuteSelection: stopped at job #{idx} due to error.");
                    break;
                }
            }
            //logger?.Log("ExecuteSelection: finished.");
        }
    }
}
