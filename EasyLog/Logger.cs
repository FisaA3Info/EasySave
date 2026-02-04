using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
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
                    _logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DailyLog");
                }
                return _logDir;
            }
            set { _logDir = value; }
        }


        //============  methods  =================
        private static string GetDailyLogPath()
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string fullPath = Path.Combine(LogDirectory, fileName);
            return fullPath;
        }

        public static void Log(LogEntry entry)
        {
            //protects from concurrent conflicts (in case but will be utile for multithreading)
            lock (_lock)
            {
                try
                {
                    string jsonLine = JsonSerializer.Serialize(entry);

                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }

                    File.AppendAllText(GetDailyLogPath(), jsonLine + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"{e.Message}");
                }
            }
        }
   }
}
