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
        stateTracker.AttachObserver(consoleProgress);


        if (args.Length > 0)
        {
            ExecuteFromArgs(args[0], manager);
        }
        else
        {
            // Injecter le manager dans la vue
            var view = new ConsoleView(manager);
            view.DisplayMenu();
        }
        ;
    }
}
