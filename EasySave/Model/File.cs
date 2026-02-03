using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal class File
    {
        public int fileSize { get; set; }
        public int progress { get; set; }
        public int nbFilesLeftToDo { get; set; }
        public int totalFilesSize { get; set; }
        public int totalFilesToCopy { get; set; }
        public string? sourceFilePath { get; set; }
        public string? targetFilePath { get; set; }
        public string? state { get; set; }
    }
}