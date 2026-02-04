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
        public void Execute(string jobName, string sourcePath, string targetPath, StateTracker stateTracker)
        {
            try
            {
                var sourceDir = new DirectoryInfo(sourcePath);

                // Verify if source directory exists
                if (!sourceDir.Exists)
                {
                    Console.WriteLine($"[Error] Source not found: {sourcePath}");
                    return;
                }

                // Create target directory
                Directory.CreateDirectory(targetPath);

                // Treat files
                foreach (var file in sourceDir.GetFiles())
                {
                    string targetFilePath = Path.Combine(targetPath, file.Name);

                    // Update state before copying
                    var activeState = new StateEntry
                    {
                        JobName = jobName,
                        TimeStamp = DateTime.Now,
                        State = BackupState.Active,
                        CurrentSourceFile = file.FullName,
                        CurrentTargetFile = targetFilePath
                    };
                    stateTracker.UpdateState(activeState);

                    // Measure time
                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    // Copy files
                    file.CopyTo(targetFilePath, true);
                    
                    timer.Stop();

                    // Log creation
                    var logEntry = new LogEntry
                    (
                        DateTime.Now,
                        jobName,
                        file.FullName,
                        targetFilePath,
                        file.Length,
                        timer.ElapsedMilliseconds
                    );

                    // Send log to the DLL
                    Logger.Log(logEntry);
                }

                // Make it recursive for subdirectories
                foreach (var subDir in sourceDir.GetDirectories())
                {
                    string newTargetDir = Path.Combine(targetPath, subDir.Name);
                    Execute(jobName, subDir.FullName, newTargetDir, stateTracker);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error: {ex.Message}");
            }
        }
    }
}
