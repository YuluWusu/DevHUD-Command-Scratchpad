using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHUD___Command_Scratchpad.Services
{
    public partial class LocalizationService : ObservableObject
    {
        public static LocalizationService Instance { get; } = new();

        [ObservableProperty]
        private string _currentLanguage = "vi";

        partial void OnCurrentLanguageChanged(string value)
        {

            OnPropertyChanged(string.Empty);
        }

        public bool IsEnglish => CurrentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

        public void ToggleLanguage()
        {
            CurrentLanguage = IsEnglish ? "vi" : "en";
        }

        public string AppTitle => IsEnglish
            ? "DevHUD - Tactical Scratchpad & Task Runner"
            : "DevHUD - Bảng Tác Vụ & Ghi Chú Chiến Thuật";

        public string LanguageBadge => IsEnglish ? "🌐 EN" : "🌐 VI";
        public string LanguageToggleToolTip => IsEnglish
            ? "Chuyển sang Tiếng Việt (Ctrl + L)"
            : "Switch to English (Ctrl + L)";

        public string AddTabButton => "+ Tab";
        public string AddTabToolTip => IsEnglish ? "Create new tab (Ctrl + N)" : "Tạo Tab mới (Ctrl + N)";

        public string SortButton => IsEnglish ? "Sort" : "Sắp xếp";
        public string SortToolTip => IsEnglish
            ? "Move completed tasks to bottom (Ctrl + Shift + S)"
            : "Đẩy việc đã xong xuống đáy (Ctrl + Shift + S)";

        public string GhostButton => "Ghost";
        public string GhostToolTip => IsEnglish
            ? "Ghost HUD overlay pinned mode (Ctrl + T)"
            : "Chế độ Ghost HUD ghim góc màn hình (Ctrl + T)";

        public string HelpButton => "?";
        public string HelpToolTip => IsEnglish
            ? "Tactical Shortcuts & Syntax Guide (Ctrl + H)"
            : "Bảng phím tắt & Hướng dẫn cú pháp (Ctrl + H)";

        public string MinimizeToolTip => IsEnglish ? "Minimize" : "Thu nhỏ";
        public string MaximizeToolTip => IsEnglish ? "Maximize / Restore" : "Phóng to / Khôi phục";
        public string CloseToolTip => IsEnglish ? "Hide / Close" : "Ẩn / Đóng";
        public string CloseTabToolTip => IsEnglish ? "Close tab" : "Đóng tab";

        public string FocusingDefault => IsEnglish ? "Focusing" : "Đang tập trung";
        public string CancelPomodoroToolTip => IsEnglish ? "Cancel Pomodoro" : "Hủy Pomodoro";

        public string GhostTitle => IsEnglish ? "👻 GHOST HUD - PENDING TASKS" : "👻 GHOST HUD - TÁC VỤ ĐANG CHỜ";
        public string FormatGhostPending(int count) => IsEnglish
            ? $"{count} tasks remaining"
            : $"{count} việc chưa hoàn thành";
        public string GhostCompleteToolTip => IsEnglish ? "Click to mark as done" : "Bấm để đánh dấu hoàn thành";
        public string GhostHabitBadge => IsEnglish ? "[H] Habit" : "[H] Thói quen";
        public string GhostLineLabel => IsEnglish ? "Line" : "Dòng";
        public string GhostFooterTip => IsEnglish
            ? "💡 Pro-tip: Press Alt + Q anywhere in Windows to summon/dismiss DevHUD instantly."
            : "💡 Mẹo: Nhấn Alt + Q để ẩn/hiện DevHUD tức thì từ bất kỳ đâu trên Windows.";

        public string ProgressLabel => IsEnglish ? "PROGRESS:" : "TIẾN ĐỘ:";
        public string StatusReady => IsEnglish ? "⚡ DevHUD Tactical Ready" : "⚡ DevHUD Sẵn Sàng Tác Chiến";
        public string StatusHabitRespawn => IsEnglish
            ? "🔥 New day active! Habit tasks [H] respawned and Streak updated."
            : "🔥 Chào ngày mới! Thói quen [H] đã tự động làm mới và cập nhật Streak.";
        public string FormatStatusSaved(string time) => IsEnglish
            ? $"💾 Auto-saved at {time}"
            : $"💾 Đã tự động lưu lúc {time}";
        public string StatusCompleted => IsEnglish ? "✔ Task marked as completed!" : "✔ Đã hoàn thành công việc!";
        public string StatusUncompleted => IsEnglish
            ? "↩ Task reverted to pending."
            : "↩ Đã chuyển công việc sang trạng thái chưa hoàn thành.";
        public string StatusSorted => IsEnglish
            ? "⚡ Completed tasks moved to the bottom."
            : "⚡ Đã gom và đẩy các việc đã xong xuống đáy tab.";
        public string FormatStatusPomodoroStarted(string title, int mins) => IsEnglish
            ? $"⏳ Pomodoro started: {title} ({mins} mins)"
            : $"⏳ Bắt đầu Pomodoro: {title} ({mins} phút)";
        public string FormatStatusPomodoroFinishedEarly(int mins) => IsEnglish
            ? $"🏆 Pomodoro completed early in {mins} minutes!"
            : $"🏆 Đã hoàn thành sớm Pomodoro trong {mins} phút!";
        public string StatusPomodoroCompleted => IsEnglish
            ? "🔔 Pomodoro time is up! Great job finishing your focus session."
            : "🔔 Hết giờ Pomodoro! Bạn đã hoàn thành phiên tập trung.";
        public string StatusPomodoroCancelled => IsEnglish ? "❌ Pomodoro cancelled." : "❌ Đã hủy Pomodoro.";

        public string BadgeDone => IsEnglish ? "Ctrl+Enter: Done" : "Ctrl+Enter: Xong";
        public string BadgePomodoro => "F5: Pomodoro";
        public string BadgeLaunch => IsEnglish ? "Ctrl+Space: Launch" : "Ctrl+Space: Mở Link";
        public string BadgeSort => IsEnglish ? "Ctrl+Shift+S: Sort" : "Ctrl+Shift+S: Gom việc";
        public string BadgeGhost => "Ctrl+T: Ghost";
        public string BadgeToggle => IsEnglish ? "Alt+Q: Toggle" : "Alt+Q: Ẩn/Hiện";

        public string HelpModalTitle => IsEnglish
            ? "⚡ DEVHUD - TACTICAL COMMAND & SHORTCUT CHEAT-SHEET"
            : "⚡ DEVHUD - BẢNG PHÍM TẮT & CÚ PHÁP TÁC CHIẾN";
        public string HelpModalSubtitle => IsEnglish
            ? "Zero-friction keyboard-driven workflow designed for high-performance developers"
            : "Giao diện bàn phím thần tốc, không độ trễ, tối ưu năng suất cho lập trình viên";
        public string HelpCloseButton => IsEnglish ? "Got it (Esc)" : "Đã hiểu (Esc)";

        public string HelpSectionHotkeys => IsEnglish ? "KEYBOARD SHORTCUTS" : "BẢNG PHÍM TẮT THẦN TỐC";
        public string HelpSectionSyntax => IsEnglish ? "INTELLIGENT SYNTAX" : "CÚ PHÁP THÔNG MINH";
        public string HelpSectionFeatures => IsEnglish ? "CORE CAPABILITIES" : "TÍNH NĂNG ĐẶC BIỆT";
    }
}
