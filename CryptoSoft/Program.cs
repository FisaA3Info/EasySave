namespace CryptoSoft;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }

            Cipher Encryption = new Cipher(args[0], args[1]);
            int EncrypTime = Encryption.SymetricalEncryption();
            Environment.Exit(EncrypTime);
        }
        catch (Exception e)
        {
            Console.WriteLine($"CryptoSoft [Error]: {e}");
            Environment.Exit(-1);
        }
    }
}