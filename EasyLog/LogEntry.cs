using System;
using System.Collections.Generic;
using System.Text;

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
        public long EncryptionTimeMs { get; set; }
        public string MachineName { get; set; } = "";
        public string UserName { get; set; } = "";

        //==============  constructor  ===============
        //required for xml (else no xml log)
        public LogEntry() { }

        public LogEntry (DateTime timeStamp, string backupName, string sourcePath, string targetPath, long fileSize, long transferTimeMs, long encryptionTimeMs, string machineName = "", string userName = "")
        {
            this.TimeStamp = timeStamp;
            this.BackupName = backupName;
            this.SourcePath = sourcePath;
            this.TargetPath = targetPath;
            this.FileSize = fileSize;
            this.TransferTimeMs = transferTimeMs;
            this.EncryptionTimeMs = encryptionTimeMs;
            this.MachineName = !string.IsNullOrEmpty(machineName) ? machineName : Environment.MachineName;
            this.UserName = !string.IsNullOrEmpty(userName) ? userName : Environment.UserName;
        }
    }
}
