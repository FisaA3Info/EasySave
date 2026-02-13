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

            //parse the args and create cipher object
            Cipher Encryption = new Cipher(args[0], args[1]);
            Console.WriteLine("CryptoSoft executing ...");
            //xor encryption
            int EncrypTime = Encryption.SymmetriclEncryption();
            Environment.Exit(EncrypTime);
        }
        catch (Exception e)
        {
            Console.WriteLine($"CryptoSoft [Error]: {e}");
            Environment.Exit(-1);
        }
    }
}