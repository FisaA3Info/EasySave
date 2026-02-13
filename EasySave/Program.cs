using EasyLog;
using EasySave.Model;
using EasySave.Service;
using EasySave.View;
using EasySave.ViewModel;

class Program
{
    static void Main(string[] args)
    {
        var settingsService = new SettingsService();
        var businessService = new BusinessSoftwareService(settingsService.Settings.BusinessSoftwareName);

        var stateTracker = new StateTracker();
        var manager = new BackupManager(stateTracker, businessService);

        var consoleProgress = new ConsoleProgressDisplay();
        stateTracker.AttachObserver(consoleProgress);

        // CLI mode or interactive mode
        if (args.Length > 0)
        {
            ExecuteFromArgs(args[0], manager);
        }
        else
        {
            var view = new ConsoleView(manager);
            view.DisplayMenu();
        }
    }

    /// Parse and execute jobs from CLI argument : - or ;
    static void ExecuteFromArgs(string arg, BackupManager manager)
    {
        //check if there are jobs
        if (manager.BackupJobs.Count == 0)
        {
            Console.WriteLine("No backup jobs found.");
            Console.WriteLine("Run EasySave without arguments to create jobs.");
            return;
        }

        // Check for range format
        if (arg.Contains("-"))
        {
            ParseAndExecuteRange(arg, manager);
        }
        // check for selection format
        else if (arg.Contains(";"))
        {
            ParseAndExecuteSelection(arg, manager);
        }
        else
        {
            if (int.TryParse(arg, out int index))
            {
                manager.ExecuteJob(index);
            }
            else
            {
                Console.WriteLine("Invalid argument: " + arg);
                DisplayUsage();
            }
        }
    }

    /// Parse range and execute
    static void ParseAndExecuteRange(string arg, BackupManager manager)
    {
        string[] parts = arg.Split('-');

        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int start) &&
            int.TryParse(parts[1], out int end))
        {
            manager.ExecuteRange(start, end);
        }
        else
        {
            Console.WriteLine("Invalid range format: " + arg);
            DisplayUsage();
        }
    }


    /// Parse selection and execute

    static void ParseAndExecuteSelection(string arg, BackupManager manager)
    {
        string[] parts = arg.Split(";");
        List<int> indices = new List<int>();

        foreach (string part in parts)
        {
            if (int.TryParse(part.Trim(), out int index))
            {
                indices.Add(index);
            }
            else
            {
                Console.WriteLine($"Invalid index ignored: {part}");
            }
        }

        if (indices.Count > 0)
        {
            manager.ExecuteSelection(indices);
        }
        else
        {
            Console.WriteLine("No valid indices found.");
            DisplayUsage();
        }
    }

    static void DisplayUsage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  EasySave.exe          Launch interactive mode");
        Console.WriteLine("  EasySave.exe 1-3      Run jobs 1 to 3");
        Console.WriteLine("  EasySave.exe 1;3      Run jobs 1 and 3");
        Console.WriteLine("  EasySave.exe 2        Run job 2");
    }
}
