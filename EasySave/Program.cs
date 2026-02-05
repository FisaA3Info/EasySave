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


        // TODO: handle args for CLI mode
        if (args.Length > 0)
        {
            ExecuteFromArgs(args, manager);
        }
        else
        {
            var view = new ConsoleView();
            view.DisplayMenu();
        }
    }

    //executes the backup from args
    static void ExecuteFromArgs(string[] args, BackupManager manager)
    {
        // TODO: need to check if jobs exist first

        string input = args[0];

        // trying to parse range
        if (input.Contains("-"))
        {
            // split
            string[] parts = input.Split("-");
            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);

            manager.ExecuteRange(start, end);
        }
        else
        {
            int index = int.Parse(input);
            manager.ExecuteJob(index);
        }
    }
}
