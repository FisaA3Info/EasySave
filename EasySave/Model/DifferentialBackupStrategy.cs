using EasyLog;
using EasySave.Service;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Linq;

namespace EasySave.Model
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        private readonly AppSettings _settings;

        //get the App settings to get the crypsoft path and the exclusion list
        public DifferentialBackupStrategy(AppSettings settings)
        {
            _settings = settings;
        }

        private BusinessSoftwareService _businessService;
        private int _totalFiles;
        private long _totalSize;
        private int _filesCopied;
        private long _sizeCopied;

        public async Task Execute(string jobName, string sourceDir, string targetDir, StateTracker stateTracker, BusinessSoftwareService businessService = null, PriorityFileManager priorityManager = null)
        {
            _businessService = businessService;
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
                else tgtDir = tgtDir.Parent;
            }

            //if so prevent from recursion
            if (isParent)
            {
                return;
            }

            // Verify if source directory exists
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
                await CopyFile(jobName, f, sourceDir, targetDir, stateTracker);
            }

            //signal that priority files are done
            if (priorityManager != null)
            {
                priorityManager.SignalPriorityDone();
            }

            //wait for all jobs to finish priority files
            if (priorityManager != null && normalFiles.Count > 0)
            {
                await priorityManager.WaitForAllPriorityAsync();
            }

            //copy non priority files
            foreach (string f in normalFiles)
            {
                await CopyFile(jobName, f, sourceDir, targetDir, stateTracker);
            }

            // Update final state
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

                //wait until business software is closed
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

                //creating directory for subdirectory, starting timer after to measure copy time only
                Directory.CreateDirectory(targetFolder);
                timer.Start();
                File.Copy(f, targetPath, true);
                timer.Stop();

                //update progress
                long fileSize = new FileInfo(targetPath).Length;
                _filesCopied++;
                _sizeCopied += fileSize;

                int progress = 0;
                if (_totalFiles > 0)
                {
                    progress = (_filesCopied * 100) / _totalFiles;
                }

                var activeState = new StateEntry
                {
                    JobName = jobName,
                    TimeStamp = DateTime.Now,
                    State = BackupState.Active,
                    TotalFiles = _totalFiles,
                    TotalSize = _totalSize,
                    Progress = progress,
                    FilesRemaining = _totalFiles - _filesCopied,
                    SizeRemaining = _totalSize - _sizeCopied,
                    CurrentSourceFile = f,
                    CurrentTargetFile = targetPath
                };

                if (stateTracker != null)
                {
                    stateTracker.UpdateState(activeState);
                }

                long encryptionTime = 0;
                FileInfo tgtFile = new FileInfo(targetPath);

                if (_settings.EncryptedExtensions.Contains(tgtFile.Extension))
                {
                    Process encryptFile = new Process();
                    encryptFile.StartInfo.CreateNoWindow = true;
                    encryptFile.StartInfo.FileName = _settings.CryptoSoftPath;
                    encryptFile.StartInfo.ArgumentList.Add(targetPath);
                    encryptFile.StartInfo.ArgumentList.Add(_settings.EncryptionKey);
                    encryptFile.Start();

                    await encryptFile.WaitForExitAsync();
                    encryptionTime = encryptFile.ExitCode;
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
            catch (Exception ex) {
                Console.WriteLine($"Error : {ex}");
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