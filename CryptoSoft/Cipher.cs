using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft
{
    internal class Cipher
    {
        private string FilePath {  get; set; }
        private string Key {  get; set; }

        //constructor taking the keys
        public Cipher (string FilePath, string Key)
        {
            this.FilePath = FilePath;
            this.Key = Key;
        }

        private bool CheckFiles()
        {
            if (!File.Exists(FilePath))
            {
                FileInfo tgtFile = new FileInfo(FilePath);
                //throw to the program error message
                throw new FileNotFoundException($"File {tgtFile.Name} not found"); 
            }
            return true;
        }

        public int SymetricalEncryption()
        {
            //if error negative time
            try
            {
                if (!CheckFiles()) { return -1; }
                //get the encryption time
                Stopwatch stopwatch = Stopwatch.StartNew();

                byte[] fileBytes = File.ReadAllBytes(FilePath);
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key);

                // XOR encryption
                for (int i = 0; i < fileBytes.Length; i++)
                {
                    fileBytes[i] ^= keyBytes[i % keyBytes.Length];
                }

                File.WriteAllBytes(FilePath, fileBytes);

                stopwatch.Stop();
                //convert the duration in ms into an int
                return (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                //throw to the program error message
                throw e; 
            }
        }
    }
}
