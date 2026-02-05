using EasyLog;
using EasySave.Model;
using EasySave.View;
using EasySave.ViewModel;

class Program
{
    static void Main()
    {
        var stateTracker = new StateTracker();
        var manager = new BackupManager(stateTracker);

        var consoleProgress = new ConsoleProgressDisplay(); 
        //stateTracker.Attach(consoleProgress); 
        

        var view = new ConsoleView();
        view.DisplayMenu();
    }
}
