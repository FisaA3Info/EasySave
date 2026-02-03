using System;
using System.Collections.Generic;
using System.Text;

namespace EasyLog
{
    internal class Logger
    {
        public string LogDirectory { get; set; }

        Logger () { }

        public static void Log(LogEntry entry)
        {

        }

        private static string GetDailyLogPath()
        {
            return "";
        }
    }
}
