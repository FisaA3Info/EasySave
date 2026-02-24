using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Model
{

    //Block parallel transfer of two large files
    public class LargeFileTransferManager
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly long _thresholdBytes;

        public LargeFileTransferManager(long thresholdKb)
        {
            _thresholdBytes = thresholdKb * 1024;
        }

        //Check if file is large
        public bool IsLargeFile(long fileSize)
        {
            return _thresholdBytes > 0 && fileSize > _thresholdBytes;
        }

        //Lock if large file until no large file is remaining
        public async Task AcquireIfLargeAsync(long fileSize)
        {
            if (IsLargeFile(fileSize))
            {
                await _semaphore.WaitAsync();
            }
        }

        //Release lock
        public void ReleaseIfLarge(long fileSize)
        {
            if (IsLargeFile(fileSize))
            {
                _semaphore.Release();
            }
        }
    }
}