using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EasyLog;
using EasySave.Service;

namespace EasySave.Model
{
    public class BackupJob : INotifyPropertyChanged, IStateObserver
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string? Name { get; set; }
        public string? SourceDir { get; set; }
        public string? TargetDir { get; set; }
        public BackupType? Type { get; set; }
        public BackupState? State { get; set; }
        internal IBackupStrategy? Strategy { get; set; }
        public JobController Controller { get; } = new JobController();
        private readonly AppSettings _settings;

        private int _progress;
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

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

        // Called by StateTracker when state changes
        public void OnStateChanged(StateEntry entry)
        {
            if (entry.JobName == Name && !Controller.IsStopped)
            {
                Progress = entry.Progress;
            }
        }

        public async Task Execute(StateTracker stateTracker, BusinessSoftwareService businessService = null, LargeFileTransferManager largeFileManager = null)
        {
            try
            {
                Progress = 0;
                State = BackupState.Active;
                Controller.Reset();

                // Listen to state changes for progress updates
                stateTracker?.AttachObserver(this);

                await Strategy.Execute(Name, SourceDir, TargetDir, stateTracker, businessService, Controller, largeFileManager);
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
                throw;
            }
            finally
            {
                stateTracker?.DetachObserver(this);
            }
        }

    }
}
