using System.Timers;

namespace Amanati.ge.Services
{
    public class OperatorIsActiveCheckTimer
    {
        private System.Timers.Timer _timer;

        public void SetTimer(double interval)
        {
            _timer ??= new System.Timers.Timer(interval);
            _timer.Elapsed += OperatorIsActiveCheckTimerElapsed;
            _timer.Enabled = true;
        }

        public void StopTimer()
        {
            if (_timer != null)
                _timer.Enabled = false;
        }

        public event Action OnElapsed;

        private void OperatorIsActiveCheckTimerElapsed(Object source, ElapsedEventArgs e)
        {
            OnElapsed?.Invoke();
            _timer.Dispose();
        }
    }
}
