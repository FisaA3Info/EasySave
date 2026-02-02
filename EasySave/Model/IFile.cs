using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    interface IFile
    {
        // Variables 
        private readonly string name;
        private readonly int fileSize;
        private readonly int progress;
        private readonly int nbFilesLeftToDo;
        private readonly int totalFilesSize;
        private readonly int totalFilesToCopy;
        private readonly string sourceFilePath;
        private readonly string targetFilePath;
        private readonly string state;
    }
}