using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DevHUD___Command_Scratchpad.Services
{
    public class HotkeyService : IHotkeyService
    {
        private const int HOTKEY_ID = 9001;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_Q = 0x51;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _hWnd;
        private HwndSource? _source;
        private bool _isRegistered;

        public event Action? GlobalHotkeyTriggered;

        public void Register(Window window)
        {
            if (_isRegistered) return;

            var helper = new WindowInteropHelper(window);
            _hWnd = helper.Handle;

            if (_hWnd == IntPtr.Zero)
            {

                window.SourceInitialized += (s, e) =>
                {
                    _hWnd = new WindowInteropHelper(window).Handle;
                    DoRegister();
                };
            }
            else
            {
                DoRegister();
            }

            window.Closed += (s, e) => Unregister();
        }

        private void DoRegister()
        {
            if (_isRegistered || _hWnd == IntPtr.Zero) return;

            _source = HwndSource.FromHwnd(_hWnd);
            _source?.AddHook(HwndHook);

            _isRegistered = RegisterHotKey(_hWnd, HOTKEY_ID, MOD_ALT, VK_Q);
            if (!_isRegistered)
            {
                int error = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Failed to register Alt+Q hotkey. Error: {error}");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                GlobalHotkeyTriggered?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Unregister()
        {
            if (_isRegistered && _hWnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hWnd, HOTKEY_ID);
                _source?.RemoveHook(HwndHook);
                _isRegistered = false;
            }
        }

        public void Dispose()
        {
            Unregister();
            GC.SuppressFinalize(this);
        }
    }
}
