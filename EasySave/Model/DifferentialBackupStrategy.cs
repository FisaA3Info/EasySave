using EasyLog;
using EasySave.Service;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;

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

        public async Task Execute(string jobName, string sourceDir, string targetDir, StateTracker stateTracker, BusinessSoftwareService businessService = null)
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

            int totalFiles = filesToCopy.Count;
            int filesCopied = 0;
            long sizeCopied = 0;

            // Update initial state
            UpdateState(jobName, stateTracker, totalFiles, totalSize, 0, 0, "", "", BackupState.Active);

            // Copy files.
            foreach (string f in filesToCopy)
            {
                // check if business software started during backup
                if (_businessService != null && _businessService.IsRunning())
                {
                    var stopLog = new LogEntry(DateTime.Now, jobName, f, "", 0, -1, 0);
                    Logger.Log(stopLog);
                    break;
                }

                string relativePath = f.Substring(sourceDir.Length + 1);
                string targetPath = Path.Combine(targetDir, relativePath);
                string? targetFolder = Path.GetDirectoryName(targetPath);

                try
                {
                    Stopwatch timer = new Stopwatch();

                    //creating directory for subdirectorys, starting timer after to measure copy time only
                    Directory.CreateDirectory(targetFolder);
                    timer.Start();
                    File.Copy(f, targetPath, true);
                    timer.Stop();

                    // update progress
                    long fileSize = new FileInfo(targetPath).Length;
                    filesCopied++;
                    sizeCopied += fileSize;

                    int progress = 0;
                    if (totalFiles > 0)
                    {
                        progress = (filesCopied * 100) / totalFiles;
                    }

                    var activeState = new StateEntry
                    {
                        JobName = jobName,
                        TimeStamp = DateTime.Now,
                        State = BackupState.Active,
                        TotalFiles = totalFiles,
                        TotalSize = totalSize,
                        Progress = progress,
                        FilesRemaining = totalFiles - filesCopied,
                        SizeRemaining = totalSize - sizeCopied,
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

                    //write logs
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
                    Console.WriteLine($"Error : {ex}");
                }
            }

            // Update final state
            UpdateState(jobName, stateTracker, totalFiles, totalSize, filesCopied, sizeCopied, "", "", BackupState.Inactive);
        }
        private void UpdateState(string jobName, StateTracker stateTracker, int totalFiles, long totalSize, int filesCopied, long sizeCopied, string sourceFile, string targetFile, BackupState state)
        {
            if (stateTracker == null) return;

            int progress = 0;
            if (totalFiles > 0)
            {
                progress = (int)((filesCopied * 100) / totalFiles);
            }

            var entry = new StateEntry
            {
                JobName = jobName,
                TimeStamp = DateTime.Now,
                State = state,
                TotalFiles = totalFiles,
                TotalSize = totalSize,
                Progress = progress,
                FilesRemaining = totalFiles - filesCopied,
                SizeRemaining = totalSize - sizeCopied,
                CurrentSourceFile = sourceFile,
                CurrentTargetFile = targetFile
            };

            stateTracker.UpdateState(entry);
        }
    }
}