using System;
using System.Collections.Generic;
using System.Text;
using EasyLog;

namespace EasySave.Model
{
    internal interface IBackupStrategy
    {
        void Execute(string sourcePath, string targetPath, Logger logger, StateTracker stateTracker);
    }
}