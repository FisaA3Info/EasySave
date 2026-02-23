using EasySave.Model;
using EasySave.Service;
using EasyLog;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.ViewModel
{
    // smaller class for the json saves (instead of full log entry)
    public class JobData
    {
        public string Name { get; set; } = "";
        public string SourceDir { get; set; } = "";
        public string TargetDir { get; set; } = "";
        public BackupType Type { get; set; }
    }

    //manages the taks/jobs 
    public class BackupManager
    {
        //=================  attributes ====================
        private StateTracker stateTracker;
        private BusinessSoftwareService _businessSoftwareService;
        private AppSettings settings;
        public List<BackupJob> BackupJobs { get; set; }
        private LargeFileTransferManager _largeFileTransferManager;

        // path to the json that contains the jobs
        private static readonly string JobsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "jobs.json"
        );

        //================ Constructor ===================
        public BackupManager(StateTracker stateTracker = null, AppSettings settings = null, BusinessSoftwareService businessSoftwareService = null)
        {
            this.stateTracker = stateTracker;
            this.settings = settings ?? new AppSettings();
            this._businessSoftwareService = businessSoftwareService;
            this._largeFileTransferManager = new LargeFileTransferManager(this.settings.MaxLargeFileTransferSizeKb);
            BackupJobs = new List<BackupJob>();
            LoadJobs();  // Load existing jobs
        }

        //================ Methods  =======================
        private void LoadJobs()
        {
            try
            {
                if (File.Exists(JobsFilePath))
                {
                    string json = File.ReadAllText(JobsFilePath);
                    var jobDataList = JsonSerializer.Deserialize<List<JobData>>(json);

                    if (jobDataList != null)
                    {
                        foreach (var data in jobDataList)
                        {
                            var job = new BackupJob(data.Name, data.SourceDir, data.TargetDir, data.Type, settings);
                            BackupJobs.Add(job);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while loading jobs: {ex.Message}");
            }
        }

        private void SaveJobs()
        {
            try
            {
                // crete the path if doesn't exists
                string? directory = Path.GetDirectoryName(JobsFilePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var jobDataList = new List<JobData>();
                foreach (var job in BackupJobs)
                {
                    jobDataList.Add(new JobData
                    {
                        //create obj Jobdata that takes some backup jobs data
                        Name = job.Name ?? "",
                        SourceDir = job.SourceDir ?? "",
                        TargetDir = job.TargetDir ?? "",
                        Type = job.Type ?? BackupType.Full
                    });
                }

                // searialize and write in json
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(jobDataList, options);
                File.WriteAllText(JobsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving jobs: {ex.Message}");
            }
        }


        //Use try catch for the error management (Maybe Error class ?)
        public bool CreateJob(string jobName, string sourcePath, string destinationPath, BackupType type)
        {

            if (string.IsNullOrWhiteSpace(jobName) || string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
            {
                return false;
            }

            try
            {
                //+ add it to the json
                var newJob = new BackupJob(jobName, sourcePath, destinationPath, type, settings);
                BackupJobs.Add(newJob);
                stateTracker?.UpdateState(new StateEntry(newJob.Name ?? jobName, BackupState.Inactive));
                SaveJobs();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //Deletes an obkect based on the index
        public bool DeleteJob(int index)
        {
            if (index < 1 || index > BackupJobs.Count)
            {
                return false;
            }

            try
            {
                //deletes the job object and removes it from the list + update json
                var removed = BackupJobs[index - 1];
                BackupJobs.RemoveAt(index - 1);
                stateTracker?.RemoveState(removed?.Name ?? string.Empty);
                SaveJobs();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //uses the managejob method to execute itself
        public async Task<bool> ExecuteJob(int index)
        {

            if (index < 1 || index > BackupJobs.Count)
            {
                return false;
            }

            var job = BackupJobs[index - 1];
            try
            {
                await job.Execute(stateTracker, _businessSoftwareService, _largeFileTransferManager);
                return true;
            }
            catch (Exception e)
            {
                var errEntry = new StateEntry(job.Name ?? string.Empty, BackupState.OnError);
                stateTracker?.UpdateState(errEntry);
                return false;
            }
        }

        //executes all the jobs in the list
        public async Task ExecuteAllJobs()
        {
            var tasks = new List<Task>();
            for (int i = 1; i <= BackupJobs.Count; i++)
            {
                tasks.Add(ExecuteJob(i));
            }
            await Task.WhenAll(tasks);
        }

        //executes a range of jobs
        public async Task ExecuteRange(int start, int end)
        {
            if (BackupJobs.Count == 0)
            {
                return;
            }

            if (start > end) (start, end) = (end, start);

            start = Math.Max(1, start);
            end = Math.Min(BackupJobs.Count, end);

            var tasks = new List<Task>();
            for (int i = start; i <= end; i++)
            {
                tasks.Add(ExecuteJob(i));
            }
            await Task.WhenAll(tasks);
        }

        //executes specific jobs by its index
        public async Task ExecuteSelection(List<int> indexes)
        {
            if (indexes == null || indexes.Count == 0)
            {
                return;
            }

            var normalized = indexes
                .Where(i => i >= 1 && i <= BackupJobs.Count)
                .Distinct()
                .OrderBy(i => i);

            if (!normalized.Any())
            {
                return;
            }

            var tasks = new List<Task>();
            foreach (var idx in normalized)
            {
                int index = idx;
                tasks.Add(ExecuteJob(index));
            }
            await Task.WhenAll(tasks);
        }
    }
}