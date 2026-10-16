using System;
using DevHUD___Command_Scratchpad.Models;

namespace DevHUD___Command_Scratchpad.Services
{
    public interface IPomodoroService
    {
        PomodoroSession? CurrentSession { get; }
        bool IsRunning { get; }

        event Action<PomodoroSession>? Tick;
        event Action<PomodoroSession>? Completed;
        event Action<PomodoroSession, int>? FinishedEarly;
        event Action? Cancelled;

        void Start(string tabId, int lineNumber, string taskTitle, int minutes = 25);
        void TogglePause();
        int FinishEarly();
        void Cancel();
    }
}
