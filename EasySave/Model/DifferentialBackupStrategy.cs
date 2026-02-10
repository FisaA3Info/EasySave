using EasyLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EasySave.Model
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(string jobName, string sourceDir, string targetDir, StateTracker stateTracker)
        {
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

            // Copy files.
            foreach (string f in filesToCopy)
            {
                string fName = f.Substring(sourceDir.Length + 1);
                string sourcePath = Path.Combine(sourceDir, fName);
                string targetPath = Path.Combine(targetDir, fName);
                string? targetFolder = Path.GetDirectoryName(targetPath);

                try
                {
                    Stopwatch timer = new Stopwatch();

                    //creating directory for subdirectorys, starting timer after to measure copy time only
                    Directory.CreateDirectory(targetFolder);
                    timer.Start();
                    File.Copy(sourcePath, targetPath, true);
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
                        CurrentSourceFile = fName,
                        CurrentTargetFile = targetPath
                    };

                    if (stateTracker != null)
                    {
                        stateTracker.UpdateState(activeState);
                    }

                    //write logs
                    var logEntry = new LogEntry
                    (
                        DateTime.Now,
                        jobName,
                        fName,
                        targetPath,
                        fileSize,
                        timer.ElapsedMilliseconds
                    );

                    Logger.Log(logEntry);
                }
                catch (DirectoryNotFoundException dirNotFound)
                {
                    Console.WriteLine(dirNotFound.Message);
                }
            }
        }
    }
}
