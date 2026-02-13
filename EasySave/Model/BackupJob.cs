using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EasyLog;
using EasySave.Service;

namespace EasySave.Model
{
    internal class BackupJob
    {
        public string? Name { get; set; }
        public string? SourceDir { get; set; }
        public string? TargetDir { get; set; }
        public BackupType? Type { get; set; }
        public BackupState? State { get; set; }
        public IBackupStrategy? Strategy { get; set; }
        private readonly AppSettings _settings;

        public BackupJob(string name, string sourceDir, string targetDir, BackupType type, AppSettings settings)
        {
            Name = name;
            SourceDir = sourceDir;
            TargetDir = targetDir;
            Type = type;
            State = BackupState.Inactive;
            _settings = settings;

            if (type == BackupType.Full)
                Strategy = new FullBackupStrategy(_settings);
            else
                Strategy = new DifferentialBackupStrategy(_settings);
        }

        public void Execute(StateTracker stateTracker, BusinessSoftwareService businessService = null)
        {
            try
            {
                State = BackupState.Active;
                Strategy.Execute(Name, SourceDir, TargetDir, stateTracker, businessService);
                State = BackupState.Inactive;
            }
            catch (Exception)
            {
                State = BackupState.OnError;

                // Update state tracker on error
                if (stateTracker != null)
                {
                    var errorEntry = new StateEntry
                    {
                        JobName = Name ?? "",
                        TimeStamp = DateTime.Now,
                        State = BackupState.OnError,
                        CurrentSourceFile = "",
                        CurrentTargetFile = ""
                    };
                    stateTracker.UpdateState(errorEntry);
                }
                // send the exception in the try catch executeJob in backup manager
                throw;
            }
        }

    }
}
