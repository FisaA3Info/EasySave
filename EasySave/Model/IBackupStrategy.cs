using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    public interface IBackupStrategy
    {
        void Execute(string sourcePath, string targetPath, Logger logger, StateTracker stateTracker);
    }
}