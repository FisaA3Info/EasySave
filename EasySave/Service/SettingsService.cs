using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using EasySave.Model;

namespace EasySave.Service
{
    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "settings.json"
        );

        public AppSettings Settings { get; private set; }

        public SettingsService()
        {
            Settings = Load();
        }

        public AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string? dir = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Settings, options));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
