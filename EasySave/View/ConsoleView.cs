using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel;
namespace EasySave.View
{
    public class ConsoleView
    {
        private readonly LanguageManager language = new LanguageManager();
        private readonly StateTracker stateTracker = new StateTracker();
        private readonly BackupManager backupManager = new BackupManager();

        public ConsoleView()
        {
            backupManager = new BackupManager(stateTracker);
            Console.WriteLine($"State file: {stateTracker.FilePath}");
        }
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
                DisplayMessage("execute_range");
                DisplayMessage("execute_specific");
                DisplayMessage("delete_jobs");
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
            Console.WriteLine();

        }

        public bool IsThereAJob()
        {
            var jobs = backupManager.BackupJobs;
            if (jobs == null || jobs.Count == 0)
            {
                DisplayMessage("no_jobs");
                return false;
            }
            else
            {
                DisplayAllJobs();
                return true;
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
                    DisplayMessage("prompt_name");
                    string name = Console.ReadLine();

                    DisplayMessage("prompt_source");
                    string source = Console.ReadLine();

                    DisplayMessage("prompt_target");
                    string target = Console.ReadLine();

                    DisplayMessage("prompt_type");
                    string TypeChoice = Console.ReadLine();

                    if (TypeChoice != "1" && TypeChoice != "2")
                    {
                        DisplayMessage("error_created");
                        break;
                    }
                    BackupType type = TypeChoice == "1" ? BackupType.Full : BackupType.Differential;

                    if ((name != null) && (source != null) && (target != null))
                    {
                        bool success = backupManager.CreateJob(name, source, target, type);
                        DisplayMessage(success ? "success_created" : "error_created");
                    }
                    else
                    {
                        DisplayMessage("error_created");
                    }
                    break;

                case "2":
                    // CHOICE EXECUTE BACKUP
                    if (IsThereAJob() == true)
                    {
                        string response = Console.ReadLine();
                        if (string.IsNullOrEmpty(response))
                        {
                            DisplayMessage("invalid_choice");
                            break;
                        }
                        int index = int.Parse(response);
                        backupManager.ExecuteJob(index);
                    }
                    break;

                case "3":
                    // CHOICE EXECUTE ALL 
                    backupManager.ExecuteAllJobs();
                    break;


                case "4":
                    // CHOICE EXECUTE RANGE 
                    if (IsThereAJob() == true)
                    {
                        DisplayMessage("nb1_range");
                        string nb_range1 = Console.ReadLine();
                        DisplayMessage("nb2_range");
                        string nb_range2 = Console.ReadLine();
                        if (string.IsNullOrEmpty(nb_range1) || string.IsNullOrEmpty(nb_range2))
                        {
                            DisplayMessage("invalid_choice");
                            break;
                        }
                        int nb_start_range = int.Parse(nb_range1);
                        int nb_end_range = int.Parse(nb_range2);
                        backupManager.ExecuteRange(nb_start_range, nb_end_range);
                    }
                    break;

                case "5":
                    // CHOICE EXECUTE SPECIFIC JOB 
                    //if(IsThereAJob() == true)
                    //{
                    //    List<string> list_string = new List<string>();
                    //    string nb_selection2 = Console.ReadLine();
                    //    if (string.IsNullOrEmpty(nb_selection1) || string.IsNullOrEmpty(nb_selection2))
                    //    {
                    //        DisplayMessage("invalid_choice");
                    //        break;
                    //    }
                    //    int nb_start_selection = int.Parse(nb_selection1);
                    //    int nb_end_selection = int.Parse(nb_selection2);
                    //    backupManager.ExecuteRange(nb_selection1, nb_selection2);
                    //}
                    break;

                case "6":
                    if (IsThereAJob() == true)
                    {
                        DisplayMessage("delete_job");
                        string response_delete = Console.ReadLine();
                        int index_delete;
                        try
                        {
                            if (string.IsNullOrEmpty(response_delete))
                            {
                                DisplayMessage("invalid_choice");
                                break;
                            }
                            index_delete = int.Parse(response_delete);
                        }
                        catch (FormatException)
                        {
                            DisplayMessage("invalid_choice");
                            break;
                        }

                        bool success_delete = backupManager.DeleteJob(index_delete);
                        if (success_delete == false)
                            DisplayMessage("error_delete");
                    }
                    break;

                case "7":
                    // CHOICE DISPLAY JOBS
                    IsThereAJob();
                    break;

                case "8":
                    // CHOICE CHANGE LANGUE
                    SelectLanguage();
                    break;

                case "9":
                    // CHOICE QUIT
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