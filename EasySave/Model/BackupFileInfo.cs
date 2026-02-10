using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EasySave.Model
{
    internal static class BackupFileInfo
    {
        public static (int fileCount, long totalSize) CalculateDirectoryStats(string path)
        {
            int count = 0;
            long size = 0;

            try
            {
                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    count++;
                    var fileInfo = new FileInfo(file);
                    size += fileInfo.Length;
                }
            }
            catch (Exception)
            {

            }
            return (count, size);
        }
    }
}