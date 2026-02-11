using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

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
        private static string _logType;

        /// <summary>
        /// 
        /// Choose between .json and .xml daily logs,
        /// json as default. Logger.LogType = value to change
        /// 
        /// </summary>
        public static string LogType 
        {
            get
            {
                if (_logType == null)
                    _logType = "json";   //json by default
                return _logType;
            }
            set { _logType = value; }
        }

        public static string LogDirectory
        {
            get
            {
                if (_logDir == null)
                    _logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "DailyLog");
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
            // look for the log type to know the extension
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.{LogType}";
            string fullPath = Path.Combine(LogDirectory, fileName);
            return fullPath;
        }

        /// <summary>
        /// 
        /// Singleton that creates or write in the Daily Log file
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public static void Log(LogEntry entry)
        {
            //protects from concurrent conflicts (in case but will be utile for multithreading)
            lock (_lock)
            {
                try
                {
                    string logContent = "";

                    // check if json or xml logs
                    if (LogType != null && LogType == "json")
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };

                        if (!Directory.Exists(LogDirectory))
                        {
                            Directory.CreateDirectory(LogDirectory);
                        }

                        string path = GetDailyLogPath();
                        List<LogEntry> entries = new List<LogEntry>();

                        // Read existing if file already exists
                        if (File.Exists(path))
                        {
                            string existingJson = File.ReadAllText(path);
                            if (!string.IsNullOrWhiteSpace(existingJson))
                            {
                                entries = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                            }
                        }

                        // Add new entry and write the full array
                        entries.Add(entry);
                        File.WriteAllText(path, JsonSerializer.Serialize(entries, options));
                        return;
                    }

                    if (LogType != null && LogType == "xml")
                    {
                        string xmlLine;
                        XmlSerializer serializer = new XmlSerializer(typeof(LogEntry));

                        using (var strwriter = new StringWriter())
                        {
                            using (XmlTextWriter writer = new XmlTextWriter(strwriter) { Formatting = Formatting.Indented })
                            {
                                serializer.Serialize(writer, entry);
                                xmlLine = strwriter.ToString();
                            }
                        }

                        if (!Directory.Exists(LogDirectory))
                        {
                            Directory.CreateDirectory(LogDirectory);
                        }
                        logContent = xmlLine;
                    }
                    // single call to get the content by stocking it in local string
                    File.AppendAllText(GetDailyLogPath(), logContent + Environment.NewLine + Environment.NewLine); //for pagingation double backrow
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"{e.Message}");
                }
            }
        }
    }
}