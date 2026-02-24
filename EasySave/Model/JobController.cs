using System.Threading;

namespace EasySave.Model
{
    public class JobController
    {
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);
        private bool _isStopped = false;

        public bool IsPaused => !_pauseEvent.IsSet;
        public bool IsStopped => _isStopped;

        public void Pause()
        {
            _pauseEvent.Reset();
        }

        public void Resume()
        {
            _pauseEvent.Set();
        }

        public void Stop()
        {
            _isStopped = true;
            _pauseEvent.Set();
        }

        public void WaitIfPaused()
        {
            _pauseEvent.Wait();
        }

        public void Reset()
        {
            _isStopped = false;
            _pauseEvent.Set();
        }
    }
}
