using System.Threading;

namespace EasySave.Model
{
    public class JobController
    {
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

        public bool IsPaused => !_pauseEvent.IsSet;

        public void Pause()
        {
            _pauseEvent.Reset();
        }

        public void Resume()
        {
            _pauseEvent.Set();
        }

        public void WaitIfPaused()
        {
            _pauseEvent.Wait();
        }

        public void Reset()
        {
            _pauseEvent.Set();
        }
    }
}
