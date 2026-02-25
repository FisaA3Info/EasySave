using System;
using System.Threading.Tasks;
using EasyLog;
using EasySave.Service;

namespace EasySave.Model
{
    internal interface IBackupStrategy
    {
        Task Execute(string jobName, string sourcePath, string targetPath, StateTracker stateTracker, BusinessSoftwareService businessService = null, JobController controller = null, LargeFileTransferManager largeFileManager = null, PriorityFileManager priorityManager = null);
    }
}