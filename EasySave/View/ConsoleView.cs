using EasySave.Service;
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
                Console.Clear();
                Console.WriteLine(language.GetText("menu_title"));
                Console.WriteLine("1. " + language.GetText("create_backup"));
                Console.WriteLine("2. " + language.GetText("execute_backup"));
                Console.WriteLine("3. " + language.GetText("execute_all"));
                Console.WriteLine("4. " + language.GetText("change_language"));
                Console.WriteLine("5. " + language.GetText("quit"));
                Console.Write(language.GetText("your_choice") + " ");

                IsRunning = UserInput();
            }
        }

        public void DisplayMessage(string key)
        {
            Console.WriteLine(language.GetText(key));
        }

        public bool UserInput()
        {
            string choice = Console.ReadLine();
            Console.WriteLine();
            switch (choice)
            {
                case "1":
                    // CHOIX CREATION BACKUP
                    break;
                case "2":
                    // CHOIX EXECUTE BACKUP
                    break;
                case "3":
                    // CHOIX EXECUTE ALL 
                    break;
                case "4":
                    // CHOIX CHANGE LANGUE
                    SelectLanguage();
                    break;
                case "5":
                    // CHOIX QUITTER
                    return false;
                default:
                    Console.WriteLine(language.GetText("invalid_choice"));
                    break;
            }

            Console.WriteLine(language.GetText("press_key"));
            Console.ReadKey();
            return true;
        }

        public void SelectLanguage()
        {
            Console.Clear();
            Console.WriteLine(language.GetText("select_language"));
            Console.WriteLine();
            Console.WriteLine("1. " + language.GetText("lang_french"));
            Console.WriteLine("2. " + language.GetText("lang_english"));
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
                    Console.WriteLine(language.GetText("invalid_choice"));
                    break;
            }
        }

    }
}
