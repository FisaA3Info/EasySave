using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EasyLog
{
    public class LogEntry
    {
        //==============  attributes  =============
        public DateTime TimeStamp { get; set;  }
        public string BackupName { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }

        //==============  constructor  ===============
        public LogEntry (DateTime timeStamp, string backupName, string sourcePath, string targetPath, long fileSize, long transferTimeMs)
        {
            TimeStamp = timeStamp;
            BackupName = backupName;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            FileSize = fileSize;
            TransferTimeMs = transferTimeMs;
        }
    }
}
