using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal class BackupFileInfo
    {
        public int FileSize { get; set; }
        public int FileName { get; set; }
        public string? SourcePath { get; set; }
        public string? TargetPath { get; set; }
    }
}