using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel;
namespace EasySave.View
{
    public class ConsoleView
    {
        private readonly LanguageManager language = new LanguageManager();
        private readonly BackupManager backupManager = new BackupManager();
        public void DisplayMenu()
        {
            bool IsRunning = true;
            while (IsRunning)
            {
                Console.Clear();
                DisplayMessage("menu_title");
                DisplayMessage("create_backup");
                DisplayMessage("execute_backup");
                DisplayMessage("execute_all");
                DisplayMessage("list_jobs");
                DisplayMessage("change_language");
                DisplayMessage("quit");
                Console.Write(language.GetText("your_choice") + " ");

                IsRunning = UserInput();
            }
        }

        public void DisplayMessage(string key)
        {
            Console.WriteLine(language.GetText(key));
        }


        public void DisplayAllJobs()
        {
            var jobs = backupManager.BackupJobs;
            if (jobs == null || jobs.Count == 0)
            {
                DisplayMessage("no_jobs");
                return;
            }

            DisplayMessage("jobs_header");
            for (int i = 0; i < jobs.Count; i++)
            {
                var j = jobs[i];
                Console.WriteLine($"{i + 1}. {j?.Name} | {j?.Type} | {language.GetText("prompt_source")} {j?.SourceDir} | {language.GetText("prompt_target")} {j?.TargetDir}");
            }
        }


        public bool UserInput()
        {
            string choice = Console.ReadLine();
            Console.WriteLine();
            switch (choice)
            {
                case "1":
                    // Ask Job info and create
                    Console.Write(language.GetText("prompt_name"));
                    string name = Console.ReadLine();

                    Console.Write(language.GetText("prompt_source"));
                    string source = Console.ReadLine();

                    Console.Write(language.GetText("prompt_target"));
                    string target = Console.ReadLine();

                    Console.Write(language.GetText("prompt_type"));
                    string TypeChoice = Console.ReadLine();

                    BackupType type = TypeChoice == "1" ? BackupType.Full : BackupType.Differential;

                    bool success = backupManager.CreateJob(name, source, target, type);
                    DisplayMessage(success ? "success_created" : "error_created");
                    break;

                case "2":
                    // CHOIX EXECUTE BACKUP
                    DisplayAllJobs();
                    int index = int.Parse(Console.ReadLine());
                    backupManager.ExecuteJob(index);
                    break;

                case "3":
                    // CHOIX EXECUTE ALL 
                    backupManager.ExecuteAllJobs();
                    break;

                case "4":
                    // CHOIX CHANGE LANGUE
                    DisplayAllJobs();
                    break;

                case "5":
                    // CHOIX CHANGE LANGUE
                    SelectLanguage();
                    break;

                case "6":
                    // CHOIX QUITTER
                    return false;
                default:
                    DisplayMessage("invalid_choice");
                    break;
            }

            DisplayMessage("press_key");
            Console.ReadKey();
            return true;
        }

        public void SelectLanguage()
        {
            Console.Clear();
            DisplayMessage("select_language");
            Console.WriteLine();
            DisplayMessage("lang_french");
            DisplayMessage("lang_english");
            Console.Write(language.GetText("your_choice") + " ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    language.LoadLanguage("fr");
                    break;
                case "2":
                    language.LoadLanguage("en");
                    break;
                default:
                    DisplayMessage("invalid_choice");
                    break;
            }
        }

    }
}
