using EasySave.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
        public BackupManager() { }


        //================ Methods  =======================
        //Use try catch for the error management (Maybe Error class ?)
        public bool CreateJob(string jobName, string sourcePath, string destinationPath, BackupType type)
        {
            //uses the managejob constructor if less than 5 jobs
            try
            {
                if (BackupJobs.Count <= 5)
                {
                    BackupJob newJob = new BackupJob(Name => jobName, SourceDir => sourcePath, TargetDir => destinationPath, Type => type);
                    BackupJobs.Add(newJob);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //Deletes an obkect based on the index
        public bool DeleteJob(int index)
        {
            try
            {
                //deletes the job object and removes it from the list²
                BackupJobs[index] = null;
                BackupJobs.RemoveAt(index);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //uses the managejob method to execute itself
        public bool ExecuteJob(int index)
        {
            try
            {
                BackupJobs[index].Execute(logger, stateTracker);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //executes all the jobs in the list
        public void ExecuteAllJobs()
        {
            try
            {
                foreach (BackupJob job in BackupJobs)
                {
                    job.Execute(logger, stateTracker);
                }

            }
            catch (Exception e)
            {

            }
        }

        //executes a range of jobs
        public void ExecuteRange(int start, int end)
        {
            try
            {
                for (int i = start; i <= end; i++)
                {
                    if (!ExecuteJob(i))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        //executes specific jobs by its index
        public void ExecuteSelection(List<int> indexes)
        {
            try
            {
                foreach (int i in indexes)
                {
                    if (!ExecuteJob(i))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
