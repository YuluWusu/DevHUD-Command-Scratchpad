using System;
using System.Media;
using System.Windows.Threading;
using DevHUD___Command_Scratchpad.Models;

namespace DevHUD___Command_Scratchpad.Services
{
    public class PomodoroService : IPomodoroService
    {
        private readonly DispatcherTimer _timer;
        private PomodoroSession? _currentSession;

        public PomodoroSession? CurrentSession => _currentSession;
        public bool IsRunning => _currentSession != null && _currentSession.IsActive && !_currentSession.IsPaused;

        public event Action<PomodoroSession>? Tick;
        public event Action<PomodoroSession>? Completed;
        public event Action<PomodoroSession, int>? FinishedEarly;
        public event Action? Cancelled;

        public PomodoroService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
        }

        public void Start(string tabId, int lineNumber, string taskTitle, int minutes = 25)
        {
            _timer.Stop();

            _currentSession = new PomodoroSession
            {
                TabId = tabId,
                LineNumber = lineNumber,
                TaskTitle = taskTitle,
                TotalSeconds = minutes * 60,
                RemainingSeconds = minutes * 60,
                IsActive = true,
                IsPaused = false,
                StartTime = DateTime.Now
            };

            _timer.Start();
            Tick?.Invoke(_currentSession);
        }

        public void TogglePause()
        {
            if (_currentSession == null) return;

            _currentSession.IsPaused = !_currentSession.IsPaused;
            if (_currentSession.IsPaused)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Start();
            }
            Tick?.Invoke(_currentSession);
        }

        public int FinishEarly()
        {
            if (_currentSession == null) return 0;

            _timer.Stop();
            int elapsedMinutes = _currentSession.ElapsedMinutes;
            var session = _currentSession;
            _currentSession = null;

            PlayBeep();
            FinishedEarly?.Invoke(session, elapsedMinutes);
            return elapsedMinutes;
        }

        public void Cancel()
        {
            _timer.Stop();
            _currentSession = null;
            Cancelled?.Invoke();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (_currentSession == null || !_currentSession.IsActive || _currentSession.IsPaused) return;

            _currentSession.RemainingSeconds--;

            if (_currentSession.RemainingSeconds <= 0)
            {
                _timer.Stop();
                var session = _currentSession;
                _currentSession = null;

                PlayNotification();
                Completed?.Invoke(session);
            }
            else
            {
                Tick?.Invoke(_currentSession);
            }
        }

        private void PlayNotification()
        {
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch { }
        }

        private void PlayBeep()
        {
            try
            {
                SystemSounds.Beep.Play();
            }
            catch { }
        }
    }
}
