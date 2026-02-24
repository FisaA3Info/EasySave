using EasyLog;
using EasySave.Service;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Model
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        private readonly AppSettings _settings;
        private BusinessSoftwareService _businessService;
        private JobController _controller;
        private LargeFileTransferManager _largeFileManager;
        private PriorityFileManager _priorityManager;
        private int _totalFiles;
        private long _totalSize;
        private int _filesCopied;
        private long _sizeCopied;

        public DifferentialBackupStrategy(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task Execute(string jobName, string sourceDir, string targetDir, StateTracker stateTracker, BusinessSoftwareService businessService = null, JobController controller = null, LargeFileTransferManager largeFileManager = null, PriorityFileManager priorityManager = null)
        {
            _businessService = businessService;
            _controller = controller;
            _largeFileManager = largeFileManager;
            _priorityManager = priorityManager;

            //check if target in source
            DirectoryInfo srcDir = new DirectoryInfo(sourceDir);
            DirectoryInfo tgtDir = new DirectoryInfo(targetDir);

            bool isParent = false;
            while (tgtDir.Parent != null)
            {
                if (tgtDir.Parent.FullName == srcDir.FullName)  //check if the parent folder is the source and repeat until root
                {
                    isParent = true;
                    break;
                }
                tgtDir = tgtDir.Parent;
            }

            if (isParent) return;

            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine($"[Error] Source not found: {sourceDir}");
                return;
            }

            string[] fileList = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            // find files that need to be copied
            List<string> filesToCopy = new List<string>();
            long totalSize = 0;

            foreach (string f in fileList)
            {
                string fName = f.Substring(sourceDir.Length + 1);
                string targetPath = Path.Combine(targetDir, fName);

                if (!File.Exists(targetPath) || File.GetLastWriteTime(f) > File.GetLastWriteTime(targetPath))
                {
                    filesToCopy.Add(f);
                    totalSize += new FileInfo(f).Length;
                }
            }

            _totalFiles = filesToCopy.Count;
            _totalSize = totalSize;
            _filesCopied = 0;
            _sizeCopied = 0;

            // Update initial state
            UpdateState(jobName, stateTracker, "", "", BackupState.Active);

            //sort priority files first then non priority files
            var priorityExts = _settings.PriorityExtensions ?? new List<string>();
            var priorityFiles = filesToCopy.Where(f => priorityExts.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)).ToList();
            var normalFiles = filesToCopy.Where(f => !priorityExts.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)).ToList();

            //copy priority files first
            foreach (string f in priorityFiles)
            {
                // Wait if paused by user
                _controller?.WaitIfPaused();
                if (_controller != null && _controller.IsStopped)
                {
                    UpdateState(jobName, stateTracker, "", "", BackupState.Inactive);
                    return;
                }

                await CopyFile(jobName, f, sourceDir, targetDir, stateTracker);
            }

            //signal that priority files are done
            _priorityManager?.SignalPriorityDone();

            //wait for all jobs to finish priority files
            if (_priorityManager != null && normalFiles.Count > 0)
            {
                await _priorityManager.WaitForAllPriorityAsync();
            }

            //copy non priority files
            foreach (string f in normalFiles)
            {
                _controller?.WaitIfPaused();
                if (_controller != null && _controller.IsStopped)
                {
                    UpdateState(jobName, stateTracker, "", "", BackupState.Inactive);
                    return;
                }
                await CopyFile(jobName, f, sourceDir, targetDir, stateTracker);
            }

            UpdateState(jobName, stateTracker, "", "", BackupState.Inactive);
        }

        private async Task CopyFile(string jobName, string f, string sourceDir, string targetDir, StateTracker stateTracker)
        {
            //check if business software started during backup and pause if needed
            if (_businessService != null && _businessService.IsRunning())
            {
                UpdateState(jobName, stateTracker, f, "", BackupState.Paused);
                var pauseLog = new LogEntry(DateTime.Now, jobName, f, "", 0, -2, 0);
                Logger.Log(pauseLog);

                while (_businessService.IsRunning())
                {
                    await Task.Delay(1000);
                }
                UpdateState(jobName, stateTracker, f, "", BackupState.Active);
            }

            string relativePath = f.Substring(sourceDir.Length + 1);
            string targetPath = Path.Combine(targetDir, relativePath);
            string? targetFolder = Path.GetDirectoryName(targetPath);

            try
            {
                Stopwatch timer = new Stopwatch();
                Directory.CreateDirectory(targetFolder);
                timer.Start();

                long currentFileSize = new FileInfo(f).Length;
                if (_largeFileManager != null)
                    await _largeFileManager.AcquireIfLargeAsync(currentFileSize);
                try
                {
                    File.Copy(f, targetPath, true);
                }
                finally
                {
                    if (_largeFileManager != null)
                        _largeFileManager.ReleaseIfLarge(currentFileSize);
                }

                timer.Stop();

                long fileSize = new FileInfo(targetPath).Length;
                _filesCopied++;
                _sizeCopied += fileSize;

                UpdateState(jobName, stateTracker, f, targetPath, BackupState.Active);

                long encryptionTime = 0;
                FileInfo tgtFile = new FileInfo(targetPath);

                if (_settings.EncryptedExtensions.Contains(tgtFile.Extension) &&
                    !string.IsNullOrEmpty(_settings.EncryptionKey) &&
                    !string.IsNullOrEmpty(_settings.CryptoSoftPath))
                {
                    encryptionTime = await CryptoSoftManager.EncryptAsync(_settings.CryptoSoftPath, targetPath, _settings.EncryptionKey);
                }

                var logEntry = new LogEntry
                (
                    DateTime.Now,
                    jobName,
                    f,
                    targetPath,
                    fileSize,
                    timer.ElapsedMilliseconds,
                    encryptionTime
                );
                Logger.Log(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        private void UpdateState(string jobName, StateTracker stateTracker, string sourceFile, string targetFile, BackupState state)
        {
            if (stateTracker == null) return;

            int progress = 0;
            if (_totalFiles > 0)
            {
                progress = (int)((_filesCopied * 100) / _totalFiles);
            }

            var entry = new StateEntry
            {
                JobName = jobName,
                TimeStamp = DateTime.Now,
                State = state,
                TotalFiles = _totalFiles,
                TotalSize = _totalSize,
                Progress = progress,
                FilesRemaining = _totalFiles - _filesCopied,
                SizeRemaining = _totalSize - _sizeCopied,
                CurrentSourceFile = sourceFile,
                CurrentTargetFile = targetFile
            };

            stateTracker.UpdateState(entry);
        }
    }
}
