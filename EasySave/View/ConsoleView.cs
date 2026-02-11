using EasyLog;
using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel;
namespace EasySave.View
{
    internal class ConsoleView
    {
        private readonly LanguageManager language;
        private readonly BackupManager backupManager;

        public ConsoleView(BackupManager manager)
        {

            backupManager = manager;
            language = new LanguageManager();
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
                DisplayMessage("change_log");
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
                        if (!int.TryParse(response, out int index))
                        {
                            DisplayMessage("invalid_choice");
                            break;
                        }
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
                        if (!int.TryParse(nb_range1, out int nb_start_range) || !int.TryParse(nb_range2, out int nb_end_range))
                        {
                            DisplayMessage("invalid_choice");
                            break;
                        }
                        backupManager.ExecuteRange(nb_start_range, nb_end_range);
                    }
                    break;

                case "5":
                    // CHOICE EXECUTE SPECIFIC JOBS
                    if (IsThereAJob() == false)
                    {
                        break;
                    }

                    List<int> selectedJobs = new List<int>();
                    DisplayMessage("selection_instructions");
                    while (true)
                    {
                        DisplayMessage("prompt_job_number");
                        string input = Console.ReadLine();
                        // cancel if empty
                        if (string.IsNullOrEmpty(input))
                        {
                            break;
                        }
                        // verify if it's a number
                        int index;
                        bool isNumber = int.TryParse(input, out index);

                        if (isNumber == false)
                        {
                            DisplayMessage("invalid_choice");
                            continue;
                        }
                        // verifies if the job exists
                        if (index < 1 || index > backupManager.BackupJobs.Count)
                        {
                            DisplayMessage("job_not_found");
                            continue;
                        }
                        // verifies if already selected
                        if (selectedJobs.Contains(index))
                        {
                            DisplayMessage("job_already_selected");
                            continue;
                        }

                        selectedJobs.Add(index);
                        Console.WriteLine("  -> Job " + index + " added");
                    }

                    // execute if jobs
                    if (selectedJobs.Count > 0)
                    {
                        backupManager.ExecuteSelection(selectedJobs);
                    }
                    else
                    {
                        DisplayMessage("no_selection");
                    }
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
                    // CHANGE DAILY LOG FORMAT
                    SelectFormatLog();
                    break;

                case "0":
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
        public void SelectFormatLog()
        {
            Console.Clear();
            DisplayMessage("select_json_format");
            Console.Write(language.GetText("actual_log"));
            Console.WriteLine($"{Logger.LogType}");
            Console.WriteLine();
            Console.WriteLine("1 : JSON");
            Console.WriteLine("2 : XML");
            Console.Write(language.GetText("your_choice") + " ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Logger.LogType = "json";
                    break;
                case "2":
                    Logger.LogType = "xml";
                    break;
                default:
                    DisplayMessage("invalid_choice");
                    break;
            }
        }


    }
}