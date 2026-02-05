using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
    /// <summary>
    /// 
    /// DLL to Implement the Logs in a Daily Log File
    /// 
    /// Located in the System ApplicationData folder
    /// based on the notation Year-Month-Day.json format
    /// 
    /// </summary>
    //based on a signleton pattern
    public static class Logger
    {
        //============ attributes  =============
        private static readonly object _lock = new object();
        private static string _logDir;

        public static string LogDirectory 
        {
            get
            {
                if (_logDir == null)
                {
                    _logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
                }
                return _logDir;
            }
            set { _logDir = value; }
        }


        //============  methods  =================
        /// <summary>
        /// 
        /// Combines the Log Directory With the Filename
        /// to Have the Path Where to Save the Logs
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetDailyLogPath()
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string fullPath = Path.Combine(LogDirectory, fileName);
            return fullPath;
        }

        public void Log(LogEntry entry)
        {
            //protects from concurrent conflicts (in case but will be utile for multithreading)
            lock (_lock)
            {
                try
                {
                    //options for a better display
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonLine = JsonSerializer.Serialize(entry, options);

                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }

                    File.AppendAllText(GetDailyLogPath(), jsonLine + Environment.NewLine + Environment.NewLine); //for pagingation double backrow
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"{e.Message}");
                }
            }
        }
   }
}
