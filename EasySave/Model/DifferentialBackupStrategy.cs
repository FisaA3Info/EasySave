using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace EasySave.Model
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(string sourceDir, string targetDir, Logger logger, StateTracker stateTracker)
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
                        Directory.CreateDirectory(targetFolder);
                        File.Copy(sourcePath, targetPath, true);
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