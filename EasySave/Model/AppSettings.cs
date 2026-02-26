using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    public class AppSettings
    {
        // CryptoSoft
        public string CryptoSoftPath { get; set; } = @"";
        public string EncryptionKey { get; set; } = "";
        public List<string> EncryptedExtensions { get; set; } = new List<string>();

        //Big Files
        public long MaxLargeFileTransferSizeKb { get; set; } = 0; //0 = no limit

        // Business software
        public string BusinessSoftwareName { get; set; } = "";

        // Log format
        public string LogFormat { get; set; } = "json";

        //Log Centralisation
        public string LogMode { get; set; } = "local"; // "local", "centralized", "both"
        public string LogServerUrl { get; set; } = "";
        public string MachineName { get; set; } = Environment.MachineName;
        public string UserName { get; set; } = Environment.UserName;

        // Priority extensions (copied first when parallel)
        public List<string> PriorityExtensions { get; set; } = new List<string>();
    }
}
