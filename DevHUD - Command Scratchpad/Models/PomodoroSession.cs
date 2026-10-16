using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHUD___Command_Scratchpad.Models
{
    public partial class PomodoroSession : ObservableObject
    {
        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _isPaused;

        [ObservableProperty]
        private string _tabId = string.Empty;

        [ObservableProperty]
        private int _lineNumber;

        [ObservableProperty]
        private string _taskTitle = string.Empty;

        [ObservableProperty]
        private int _remainingSeconds;

        [ObservableProperty]
        private int _totalSeconds = 25 * 60;

        [ObservableProperty]
        private DateTime _startTime = DateTime.Now;

        public int ElapsedSeconds => TotalSeconds - RemainingSeconds;

        public int ElapsedMinutes => Math.Max(1, (int)Math.Round((double)ElapsedSeconds / 60));

        public string FormattedTime
        {
            get
            {
                int min = RemainingSeconds / 60;
                int sec = RemainingSeconds % 60;
                return $"{min:D2}:{sec:D2}";
            }
        }

        public string InlineTag => $"[⏳ {FormattedTime}]";

        public double ProgressPercentage => TotalSeconds > 0 ? (double)(TotalSeconds - RemainingSeconds) / TotalSeconds * 100 : 0;
    }
}
