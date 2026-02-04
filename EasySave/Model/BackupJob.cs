using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EasyLog;

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

        public BackupJob(string name, string sourceDir, string targetDir, BackupType type)
        {
            Name = name;
            SourceDir = sourceDir;
            TargetDir = targetDir;
            Type = type;
            State = null;
            Strategy = null;
        }

        public void Execute(Logger logger, StateTracker stateTracker)
        {
            State = BackupState.Active;
            Strategy.Execute(Name,SourceDir, TargetDir, logger, stateTracker);
            State = BackupState.Inactive;

        }

    }
}
