using System.Timers;

namespace Amanati.ge.Services
{
    public class ConfigPageTimer
    {
        private System.Timers.Timer _timer;

        public void SetTimer(double interval)
        {
            _timer ??= new System.Timers.Timer(interval);
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = true;
        }

        public void StopTimer()
        {
            _timer.Enabled = false;
        }

        public event Action OnElapsed;

        private void TimerElapsed(Object source, ElapsedEventArgs e)
        {
            OnElapsed?.Invoke();
            _timer.Dispose();
        }
    }
}
