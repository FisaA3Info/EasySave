using System.Diagnostics;

namespace EasySave.Service
{
    public class BusinessSoftwareService
    {
        private string _processName;

        public BusinessSoftwareService(string processName)
        {
            _processName = processName;
        }

        /// Check if the business software is currently running
        public bool IsRunning()
        {
            if (string.IsNullOrEmpty(_processName))
                return false;

            // Remove .exe extension if present
            string name = _processName.Replace(".exe", "");
            return Process.GetProcessesByName(name).Length > 0;
        }
    }
}
