using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EasyLog;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.ComponentModel.DataAnnotations;

namespace EasySave.Model
{
    internal class FullBackupStrategy : IBackupStrategy
    {
        private int _totalFiles;
        private long _totalSize;
        private int _filesCopied;
        private long _sizeCopied;

        public void Execute(string jobName, string sourcePath, string targetPath, StateTracker stateTracker)
        {
                var sourceDir = new DirectoryInfo(sourcePath);

                // Verify if source directory exists
                if (!sourceDir.Exists)
                {
                    Console.WriteLine($"[Error] Source not found: {sourcePath}");
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
                ExecuteRecursive(jobName, sourcePath, targetPath, stateTracker);

                // Update final state
                UpdateState(jobName, stateTracker, "", "", BackupState.Inactive);
            }

        private void ExecuteRecursive(string jobName, string sourcePath, string targetPath, StateTracker stateTracker)
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
                    return;
                }

                // Copy files
                foreach (var file in sourceDir.GetFiles())
                {
                    string targetFilePath = Path.Combine(targetPath, file.Name);
                    
                    // Update before copy
                    UpdateState(jobName, stateTracker, file.FullName, targetFilePath, BackupState.Active);

                    // Measure performance
                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    // Copy file
                    file.CopyTo(targetFilePath, true);

                    timer.Stop();

                    // Update stats
                    _filesCopied++;
                    _sizeCopied += file.Length;

                    // Log the copy operation
                    var logEntry = new LogEntry
                    (
                        DateTime.Now,
                        jobName,
                        file.FullName,
                        targetFilePath,
                        file.Length,
                        timer.ElapsedMilliseconds
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

                    ExecuteRecursive(jobName, subDir.FullName, newTargetDir, stateTracker);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Critical Error: {ex.Message}");
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
