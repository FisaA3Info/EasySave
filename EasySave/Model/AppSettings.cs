using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    public class AppSettings
    {
        // CryptoSoft
        public string CryptoSoftPath { get; set; } = @"C:\\Users\\theot\\source\\repos\\FisaA3Info\\EasySave\\CryptoSoft\\bin\\Debug\\net10.0\\CryptoSoft.exe";
        public string EncryptionKey { get; set; } = "miaou";
        public List<string> EncryptedExtensions { get; set; } = new List<string>();

        // Business software
        public string BusinessSoftwareName { get; set; } = "CalculatorApp.exe";

        // Log format
        public string LogFormat { get; set; } = "json";
    }
}
