using EasyLog;
using EasySave.View;
using System.Runtime.ExceptionServices;

class Program
{
    static void Main()
    {
        //EGS TO MAKE A LOG ENTRY (GO TO APPDATA/DAILYLOG)
        //var entry = new LogEntry
        //(
        //    DateTime.Now,
        //    "Save_Docs_Client",
        //    @"\\ServeurSource\Partage\monfichier.zip",
        //    @"\\ServeurDest\Backup\monfichier.zip",
        //    1024500,
        //    450 
        //);

        //Logger.Log( entry );

        var view = new ConsoleView();
        view.DisplayMenu();
    }
}
