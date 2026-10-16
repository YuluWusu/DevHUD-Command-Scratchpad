using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHUD___Command_Scratchpad.Models
{
    public partial class GhostTaskItem : ObservableObject
    {
        [ObservableProperty]
        private string _tabId = string.Empty;

        [ObservableProperty]
        private string _tabTitle = string.Empty;

        [ObservableProperty]
        private int _lineNumber;

        [ObservableProperty]
        private string _rawText = string.Empty;

        [ObservableProperty]
        private string _displayText = string.Empty;

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private bool _isHabit;

        [ObservableProperty]
        private string _streak = string.Empty;

        [ObservableProperty]
        private bool _hasActiveTimer;

        [ObservableProperty]
        private string _timerDisplay = string.Empty;
    }
}
