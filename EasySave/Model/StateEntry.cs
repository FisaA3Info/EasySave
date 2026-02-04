using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal class StateEntry
    {
        public string JobName { get; set; }
        public DateTime Timestamp { get; set;  }
        public BackupState State { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int Progress { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }
        public string CurrentSourceFile { get; set; }
        public string CurrentTargetFile { get; set; }


        public StateEntry(string jobname,BackupState state, int totalfiles, long totalsize, int filesremaining, long sizeremaining,string currentsourcefile,string currenttargetfile)
        {
            this.JobName = jobname;
            this.Timestamp = DateTime.Now();
            this.State = state;
            this.TotalFiles = totalfiles;
            this.TotalSize = totalsize;
            this.Progress = null;
            this.FilesRemaining = filesremaining;
            this.SizeRemaining = sizeremaining;
            this.CurrentSourceFile = currentsourcefile;
            this.CurrentTargetFile = currenttargetfile;
        }
    }
}
