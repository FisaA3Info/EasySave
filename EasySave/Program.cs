using EasyLog;
using EasySave.Model;
using EasySave.View;
using EasySave.ViewModel;

class Program
{
    static void Main(string[] args)
    {
        var stateTracker = new StateTracker();
        var manager = new BackupManager(stateTracker);

        var consoleProgress = new ConsoleProgressDisplay();
        //stateTracker.Attach(consoleProgress);

        // CLI mode or interactive mode
        if (args.Length > 0)
        {
            ExecuteFromArgs(args[0], manager);
        }
        else
        {
            var view = new ConsoleView();
            view.DisplayMenu();
        }
    }

    /// Parse and execute jobs from CLI argument
    static void ExecuteFromArgs(string arg, BackupManager manager)
    {
        //check if there are jobs
        if (manager.BackupJobs.Count == 0)
        {
            Console.WriteLine("pas de job");
            return;
        }

        // Check for range format
        if (arg.Contains("-"))
        {
            ParseAndExecuteRange(arg, manager);
        } else
        {
            if (int.TryParse(arg, out int index))
            {
                manager.ExecuteJob(index);
            }
            else
            {
                Console.WriteLine($"invalide: {arg}");
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
            Console.WriteLine($"mauvais range: {arg}");
            DisplayUsage();
        }
    }

    static void DisplayUsage()
    {
        Console.WriteLine();
        Console.WriteLine("EasySave.exe bonjour");
    }
}
