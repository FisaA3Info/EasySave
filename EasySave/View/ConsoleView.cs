using EasySave;
namespace EasySave.View
{
    public class ConsoleView
    {
        private readonly LanguageManager language = new LanguageManager();
        public void DisplayMenu()
        {
            bool IsRunning = true;
            while (IsRunning)
            {
                Console.WriteLine("1. Créer un travail de sauvegarde");
                Console.WriteLine("2. Exécuter un travail");
                Console.WriteLine("3. Exécuter tous les travaux");
                Console.WriteLine("4. Quitter");
                Console.WriteLine("Votre choix : ");
                UserInput();
            }
        }

        public void DisplayMessage(string key)
        {

        }

        public void UserInput()
        {
            string choice = Console.ReadLine();
            Console.WriteLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("On est dans le cas 1");
                    break;
                case "2":
                    Console.WriteLine("On est dans le cas 2");
                    break;
                case "3":
                    Console.WriteLine("On est dans le cas 3");
                    break;
                case "4":
                    Console.WriteLine("On est dans le cas 4");
                    break;
            }
        }

        public void SelectLanguage()
        {
            
        }

    }
}
