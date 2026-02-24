using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

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
        private static string _logMode = "local";  // "local", "centralized", "both"
        private static string _logServerUrl = "";
        private static readonly HttpClient _httpClient = new HttpClient();
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
        public static string LogMode
        {
            get => _logMode;
            set => _logMode = value ?? "local";
        }

        public static string LogServerUrl
        {
            get => _logServerUrl;
            set => _logServerUrl = value ?? "";
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

        private static async Task SendToServerAsync(LogEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_logServerUrl))
                return;

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = false };
                string json = JsonSerializer.Serialize(entry, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = _logServerUrl.TrimEnd('/') + "/api/logs";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Log server returned {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to send log to server: {ex.Message}");
            }
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

                    else if (LogType != null && LogType == "xml")
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
                        File.AppendAllText(GetDailyLogPath(), xmlLine + Environment.NewLine + Environment.NewLine); //for pagingation double backrow
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"{e.Message}");
                }
            }
        }
    }
}