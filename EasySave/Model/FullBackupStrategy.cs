using EasyLog;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using EasySave.Service;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;

namespace EasySave.Model
{
    internal class FullBackupStrategy : IBackupStrategy
    {
        private readonly AppSettings _settings;
        private int _totalFiles;
        private long _totalSize;
        private int _filesCopied;
        private long _sizeCopied;
        private bool _isError;

        private BusinessSoftwareService _businessService;
        private LargeFileTransferManager _largeFileManager;

        //get the App settings to get the crypsoft path and the exclusion list
        public FullBackupStrategy(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task Execute(string jobName, string sourcePath, string targetPath, StateTracker stateTracker, BusinessSoftwareService businessService = null, LargeFileTransferManager largeFileManager = null)
        {
                _businessService = businessService;
                _largeFileManager = largeFileManager;
                var sourceDir = new DirectoryInfo(sourcePath);

                // Verify if source directory exists
                if (!sourceDir.Exists)
                {
                    Console.WriteLine($"[Error] Source not found: {sourcePath}");
                    return;
                }
                // prevent progress bar display
                if (_isError)
                {
                    return;
                }

                // Caculate stats
                var (fileCount, totalSize) = BackupFileInfo.CalculateDirectoryStats(sourcePath);
                _totalFiles = fileCount;
                _totalSize = totalSize;
                _filesCopied = 0;
                _sizeCopied = 0;

                // Update initial state
                UpdateState(jobName, stateTracker, "", "", BackupState.Active);

                // Recursive execution
                await ExecuteRecursive(jobName, sourcePath, targetPath, stateTracker);

                // Update final state
                UpdateState(jobName, stateTracker, "", "", BackupState.Inactive);
            }

        private async Task ExecuteRecursive(string jobName, string sourcePath, string targetPath, StateTracker stateTracker)
        {
            try
            {
                var sourceDir = new DirectoryInfo(sourcePath);

                // Create target directory
                Directory.CreateDirectory(targetPath);

                //check if target in source
                DirectoryInfo srcDir = new DirectoryInfo(sourcePath);
                DirectoryInfo tgtDir = new DirectoryInfo(targetPath);

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
                    _isError = true;
                    UpdateState(jobName, stateTracker, "", "", BackupState.OnError);
                    return;
                }

                foreach (var file in sourceDir.GetFiles())
                {
                    // check if business software started during backup and wait until it stops
                    if (_businessService != null && _businessService.IsRunning())
                    {
                        UpdateState(jobName, stateTracker, file.FullName, "", BackupState.Paused);
                        var pauseLog = new LogEntry(DateTime.Now, jobName, file.FullName, "", 0, -2, 0);
                        Logger.Log(pauseLog);

                        //wait until business software stops (checks every second)
                        while (_businessService.IsRunning())
                        {
                            await Task.Delay(1000); 
                        }

                        //resume backup
                        UpdateState(jobName, stateTracker, file.FullName, "", BackupState.Active);
                    }


                    string targetFilePath = Path.Combine(targetPath, file.Name);

                    // Update before copy
                    UpdateState(jobName, stateTracker, file.FullName, targetFilePath, BackupState.Active);

                    // Measure performance
                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    // Copy file
                    // Lock large files
                    if (_largeFileManager != null)
                        await _largeFileManager.AcquireIfLargeAsync(file.Length);
                    try
                    {
                        // Copy file
                        file.CopyTo(targetFilePath, true);
                    }
                    finally
                    {
                        if (_largeFileManager != null)
                            _largeFileManager.ReleaseIfLarge(file.Length);
                    }

                    timer.Stop();

                    // Update stats
                    _filesCopied++;
                    _sizeCopied += file.Length;

                    long encryptionTime = 0;
                    FileInfo tgtFile = new FileInfo(targetFilePath);

                    if (_settings.EncryptedExtensions.Contains(tgtFile.Extension))
                    {
                        Process encryptFile = new Process();
                        encryptFile.StartInfo.CreateNoWindow = true;
                        encryptFile.StartInfo.FileName = _settings.CryptoSoftPath;
                        encryptFile.StartInfo.ArgumentList.Add(tgtFile.FullName);
                        encryptFile.StartInfo.ArgumentList.Add(_settings.EncryptionKey);
                        encryptFile.Start();

                        await encryptFile.WaitForExitAsync();
                        encryptionTime = encryptFile.ExitCode;
                    }

                    // Log the copy operation
                    var logEntry = new LogEntry
                    (
                        DateTime.Now,
                        jobName,
                        file.FullName,
                        targetFilePath,
                        file.Length,
                        timer.ElapsedMilliseconds,
                        encryptionTime
                    );
                    Logger.Log(logEntry);

                    // Update after copy
                    UpdateState(jobName, stateTracker, file.FullName, targetFilePath, BackupState.Active);
                }

                foreach (var subDir in sourceDir.GetDirectories())
                {
                    string newTargetDir = Path.Combine(targetPath, subDir.Name);

                    // skip if this subdirectory is the target directory itself
                    DirectoryInfo subDirPath = new DirectoryInfo(subDir.FullName);
                    DirectoryInfo targetRoot = new DirectoryInfo(targetPath);

                    if (string.Equals(subDirPath.FullName, targetRoot.FullName, StringComparison.OrdinalIgnoreCase) || subDirPath.FullName.StartsWith(targetRoot.FullName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    await ExecuteRecursive(jobName, subDir.FullName, newTargetDir, stateTracker);
                }
            }
            catch (Exception ex) {
                _isError = true;
                Console.WriteLine($"Critical Error: {ex.Message}");
                UpdateState(jobName, stateTracker, "", "", BackupState.OnError);
            }
        }

        private void UpdateState(string jobName, StateTracker stateTracker, string sourceFile, string targetFile, BackupState state)
        {
            if (stateTracker == null) return;

            // Calculate progress (%)
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
