using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;


namespace EasySave.Service
{
    internal class LanguageManager
    {
        private string currentLanguage;
        private Dictionary<string, string> translations;

        public LanguageManager()
        {
            translations = new Dictionary<string, string>();
            currentLanguage = "fr";
            LoadLanguage(currentLanguage);
        }
        public void LoadLanguage(string language)
        {
            currentLanguage = language;
            string filePath = Path.Combine("Ressources", $"{language}.json");

            try
            {
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
                                   ?? new Dictionary<string, string>();
                }
                else
                {
                    Console.WriteLine($"Fichier de langue '{filePath}' introuvable.");
                    translations = new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de la langue : {ex.Message}");
                translations = new Dictionary<string, string>();
            }
        }

        public string GetText(string key) 
        {
            if (translations.ContainsKey(key))
            {
                return translations[key];
            }
            return $"[{key}]";
        }
    }
}