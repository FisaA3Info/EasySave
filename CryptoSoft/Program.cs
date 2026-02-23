using System.Threading;

namespace CryptoSoft;

public static class Program
{
    private static Mutex _mutex;
    public static int Main(string[] args)
    {
        // 'global\\' to check on the entire machine (local is default so we add to prevent the launch on other sessions)
        _mutex = new Mutex(true, "Global\\CryptoSoftMutex" , out bool isFree);

        if (!isFree)
        {
            Console.WriteLine($"CryptoSoft isn't available right now ! Please wait...");
            _mutex.WaitOne();
        }

        try
        {
            if (args.Length < 2)
            {
                Console.WriteLine("[Error]: Invalid arguments.");
                return -1;
            }

            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }

            //parse the args and create cipher object
            Cipher Encryption = new Cipher(args[0], args[1]);
            Console.WriteLine("CryptoSoft executing ...");
            //xor encryption
            int EncrypTime = Encryption.SymmetricalEncryption();
            return EncrypTime;
        }
        catch (Exception e)
        {
            Console.WriteLine($"CryptoSoft [Error]: {e}");
            return -1;
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}