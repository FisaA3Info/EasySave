using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal class StateEntry
    {
        public string JobName { get; set; }
        public DateTime TimeStamp { get; set; }
        public BackupState State { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int Progress { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }
        public string CurrentSourceFile { get; set; }
        public string CurrentTargetFile { get; set; }

        public StateEntry(
            string jobname,
            BackupState state, 
            int totalfiles = 0, 
            long totalsize =0, 
            int filesremaining = 0, 
            long sizeremaining = 0,
            string currentsourcefile = "",
            string currenttargetfile = ""
        )
        {
            this.JobName = jobname;
            this.TimeStamp = DateTime.Now;
            this.State = state;
            this.TotalFiles = totalfiles;
            this.TotalSize = totalsize;
            this.Progress = 0;
            this.FilesRemaining = filesremaining;
            this.SizeRemaining = sizeremaining;
            this.CurrentSourceFile = currentsourcefile;
            this.CurrentTargetFile = currenttargetfile;
        }
    }
}

