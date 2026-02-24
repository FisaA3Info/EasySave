using System.Threading;

namespace CryptoSoft;

public static class Program
{
    private static Mutex _mutex;
    public static void Main(string[] args)
    {
        // 'global\\' to check on the entire machine (local is default so we add to prevent the launch on other sessions)
        _mutex = new Mutex(true, "Global\\CryptoSoftMutex" , out bool isFree);

        if (!isFree)
        {
            Console.Error.WriteLine($"CryptoSoft isn't available right now ! Please wait...");
            _mutex.WaitOne();
        }

        try
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("[Error]: Invalid arguments.");
                Console.WriteLine("-1"); ;
            }

            foreach (var arg in args)
            {
                Console.Error.WriteLine(arg);
            }

            //parse the args and create cipher object
            Cipher Encryption = new Cipher(args[0], args[1]);
            Console.Error.WriteLine("CryptoSoft executing ...");
            //xor encryption
            int EncrypTime = Encryption.SymmetricalEncryption();
            Console.WriteLine($"{EncrypTime}");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"CryptoSoft [Error]: {e}");
            Console.WriteLine("-1");
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}