using System;
using System.Windows;

namespace DevHUD___Command_Scratchpad.Services
{
    public interface IHotkeyService : IDisposable
    {
        event Action? GlobalHotkeyTriggered;
        void Register(Window window);
        void Unregister();
    }
}
