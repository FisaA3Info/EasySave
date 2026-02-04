using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
    public class Logger
    {
        private static string logDir;
        public static string LogDirectory 
        {
            // default path
            get
            {
                logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
                return logDir;
            }
            // manually added path-
            set { logDir = value; }
        }

        private static string GetDailyLogPath()
        {
            // get the full path based on the current day
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string fullPath = Path.Combine(LogDirectory, fileName);
            return fullPath;
        }

        public static void Log(LogEntry entry)
        {
            // get the entry in json
            string jsonLine = JsonSerializer.Serialize(entry);

            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            // add last data to the file
            File.AppendAllText(GetDailyLogPath(), jsonLine + Environment.NewLine);
        }
   }
}
