using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Model
{
    internal class CryptoSoftManager
    {

        //manage the cryptosoft calls, using a semaphore slim cause it's lighter
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static async Task<long> EncryptAsync(string cryptoSoftPath, string filePath, string key)
        {
            await _semaphore.WaitAsync();
            try
            {
                //call here crypto instead of the strategies logic
                using var process = new Process();
                process.StartInfo.FileName = cryptoSoftPath;
                process.StartInfo.ArgumentList.Add(filePath);
                process.StartInfo.ArgumentList.Add(key);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                //error not used, to implement in view soon pls
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var output = await outputTask;

                try
                {
                    //retrieve encryption time
                    return long.Parse(output.Trim());
                }
                catch 
                {
                    return -1;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
