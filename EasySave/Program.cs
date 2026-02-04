using EasyLog;
using EasySave.Model;
using EasySave.View;
using EasySave.ViewModel;

class Program
{
    static void Main()
    {
        var logger = new Logger();
        var stateTracker = new StateTracker();
        var manager = new BackupManager(logger, stateTracker);

        var consoleProgress = new ConsoleProgressDisplay(); 
        stateTracker.Attach(consoleProgress); 
        

        var view = new ConsoleView();
        view.DisplayMenu();
    }
}
