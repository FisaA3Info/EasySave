using EasyLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace EasySave.Model
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(string jobName, string sourceDir, string targetDir, Logger logger, StateTracker stateTracker)
        {
            string[] fileList = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            // Copy files.
            foreach (string f in fileList)
            {
                // Remove path from the file name.
                string fName = f.Substring(sourceDir.Length + 1);
                string sourcePath = Path.Combine(sourceDir, fName);
                string targetPath = Path.Combine(targetDir, fName);

                // For subdirectory -> the subdirectory might not exist on the target folder, so we would need to create it
                string? targetFolder = Path.GetDirectoryName(targetPath);

                // Use the Path.Combine method to safely append the file name to the path.

                // Looking if the file exists, if it does we look if the file has been changes. If yes we copy.
                if (!File.Exists(targetPath) || File.GetLastWriteTime(sourcePath) > File.GetLastWriteTime(targetPath)) {
                    try
                    {
                        var activeState = new StateEntry
                        {
                            JobName = jobName, // Manager will set actual job name 
                            TimeStamp = DateTime.Now,
                            State = BackupState.Active,
                            CurrentSourceFile = fName,
                            CurrentTargetFile = targetPath
                        };

                        // Updating state and creating a stopwatch for each file
                        stateTracker.UpdateState(activeState);
                        Stopwatch timer = new Stopwatch();
                        
                        //creating directory for subdirectorys, starting timer after to measure copy time only
                        Directory.CreateDirectory(targetFolder);
                        timer.Start();
                        File.Copy(sourcePath, targetPath, true);
                        timer.Stop();

                        //write logs
                        var logEntry = new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            JobName = jobName, // Manager will set actual job name 
                            SourcePath = sourcePath,
                            TargetPath = targetPath,
                            FileSize = new FileInfo(sourcePath).Length,
                            TransferTimeMs = timer.ElapsedMilliseconds
                        };
                        logger.Log(logEntry);
                    }
                    catch (DirectoryNotFoundException dirNotFound)
                    {
                        Console.WriteLine(dirNotFound.Message);
                    }
                }
            }
        }
    }
}