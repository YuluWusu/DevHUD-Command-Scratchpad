using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHUD___Command_Scratchpad.Models;
using DevHUD___Command_Scratchpad.Services;

namespace DevHUD___Command_Scratchpad.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IStorageService _storageService;
        private readonly IPomodoroService _pomodoroService;
        private readonly IQuickLaunchService _quickLaunchService;
        private readonly DispatcherTimer _autoSaveTimer;

        [ObservableProperty]
        private ObservableCollection<TabModel> _tabs = new();

        [ObservableProperty]
        private TabModel? _currentTab;

        [ObservableProperty]
        private bool _isGhostMode;

        [ObservableProperty]
        private double _ghostOpacity = 0.75;

        [ObservableProperty]
        private string _statusMessage = "⚡ DevHUD Tactical Ready";

        [ObservableProperty]
        private PomodoroSession? _activePomodoro;

        [ObservableProperty]
        private ObservableCollection<GhostTaskItem> _ghostTasks = new();

        public LocalizationService L10n => LocalizationService.Instance;

        [ObservableProperty]
        private bool _isHelpVisible;

        public Action<string>? UpdateEditorTextAction { get; set; }
        public Func<string>? GetEditorTextAction { get; set; }
        public Func<int>? GetCaretLineNumberAction { get; set; }
        public Action<int, string>? ReplaceEditorLineAction { get; set; }

        public MainViewModel(
            IStorageService storageService,
            IPomodoroService pomodoroService,
            IQuickLaunchService quickLaunchService)
        {
            _storageService = storageService;
            _pomodoroService = pomodoroService;
            _quickLaunchService = quickLaunchService;

            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(600)
            };
            _autoSaveTimer.Tick += (s, e) =>
            {
                _autoSaveTimer.Stop();
                PerformSave();
            };

            _pomodoroService.Tick += OnPomodoroTick;
            _pomodoroService.Completed += OnPomodoroCompleted;
            _pomodoroService.FinishedEarly += OnPomodoroFinishedEarly;
            _pomodoroService.Cancelled += OnPomodoroCancelled;

            InitializeData();
        }

        private void InitializeData()
        {
            var config = _storageService.LoadConfig();

            bool habitRespawned = _storageService.ProcessDailyHabitRespawn(config);

            Tabs.Clear();
            foreach (var tab in config.Tabs)
            {
                Tabs.Add(tab);
            }

            if (Tabs.Count == 0)
            {
                var seedTabs = _storageService.GetSeedData();
                foreach (var tab in seedTabs)
                {
                    Tabs.Add(tab);
                }
            }

            if (!string.IsNullOrEmpty(config.Language))
            {
                LocalizationService.Instance.CurrentLanguage = config.Language;
            }

            CurrentTab = Tabs.FirstOrDefault(t => t.Id == config.ActiveTabId) ?? Tabs.FirstOrDefault();

            if (habitRespawned)
            {
                StatusMessage = L10n.StatusHabitRespawn;
            }
        }

        partial void OnCurrentTabChanged(TabModel? value)
        {
            if (value != null)
            {
                value.RecalculateStats();
                RefreshGhostTasks();
            }
        }

        public void TriggerAutoSave()
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
        }

        public void PerformSave()
        {
            if (CurrentTab != null && GetEditorTextAction != null)
            {
                CurrentTab.Content = GetEditorTextAction();
                CurrentTab.RecalculateStats();
            }

            var config = new AppConfig
            {
                ActiveTabId = CurrentTab?.Id ?? string.Empty,
                LastActiveDate = DateTime.Today,
                Language = LocalizationService.Instance.CurrentLanguage,
                Tabs = Tabs.ToList()
            };

            _storageService.SaveConfig(config);
            StatusMessage = L10n.FormatStatusSaved(DateTime.Now.ToString("HH:mm:ss"));
        }

        [RelayCommand]
        public void ToggleLanguage()
        {
            L10n.ToggleLanguage();
            PerformSave();
            OnPropertyChanged(nameof(L10n));
            StatusMessage = L10n.IsEnglish
                ? "🌐 Language set to English."
                : "🌐 Ngôn ngữ đã đổi sang Tiếng Việt.";
        }

        [RelayCommand]
        public void ToggleHelp()
        {
            IsHelpVisible = !IsHelpVisible;
        }

        [RelayCommand]
        public void CloseHelp()
        {
            IsHelpVisible = false;
        }

        [RelayCommand]
        public void AddTab()
        {
            int index = Tabs.Count + 1;
            var newTab = new TabModel($"Tab {index}", $"# 📌 Ghi chú Tab {index}\n- Viết công việc mới ở đây\n");
            Tabs.Add(newTab);
            CurrentTab = newTab;
            PerformSave();
        }

        [RelayCommand]
        public void CloseTab(TabModel? tab)
        {
            if (tab == null || Tabs.Count <= 1) return;

            int index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);

            if (CurrentTab == tab)
            {
                int newIndex = Math.Max(0, index - 1);
                CurrentTab = Tabs[newIndex];
            }

            PerformSave();
        }

        [RelayCommand]
        public void SelectTabByIndex(int index)
        {
            if (index >= 0 && index < Tabs.Count)
            {
                CurrentTab = Tabs[index];
            }
        }

        public void ToggleCompletionAtCurrentLine()
        {
            if (CurrentTab == null || GetEditorTextAction == null || GetCaretLineNumberAction == null) return;

            int currentLineNumber = GetCaretLineNumberAction();
            string fullText = GetEditorTextAction();
            var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (currentLineNumber < 1 || currentLineNumber > lines.Length) return;

            string line = lines[currentLineNumber - 1];
            string trimmed = line.TrimStart();

            if (!trimmed.StartsWith("-", StringComparison.Ordinal)) return;

            if (_pomodoroService.IsRunning &&
                _pomodoroService.CurrentSession?.TabId == CurrentTab.Id &&
                _pomodoroService.CurrentSession?.LineNumber == currentLineNumber)
            {
                _pomodoroService.FinishEarly();
                return;
            }

            var completedRegex = new Regex(@"\s*\((?:hoàn thành|done|completed)[^)]*\)", RegexOptions.IgnoreCase);

            if (completedRegex.IsMatch(line))
            {

                string newLine = completedRegex.Replace(line, "");
                ReplaceEditorLineAction?.Invoke(currentLineNumber, newLine);
                StatusMessage = L10n.StatusUncompleted;
            }
            else
            {

                string tag = L10n.IsEnglish ? " (done)" : " (hoàn thành)";
                string newLine = $"{line.TrimEnd()}{tag}";
                ReplaceEditorLineAction?.Invoke(currentLineNumber, newLine);
                StatusMessage = L10n.StatusCompleted;
            }

            TriggerAutoSave();
            RefreshGhostTasks();
        }

        [RelayCommand]
        public void SortCurrentTabTasks()
        {
            if (CurrentTab == null || GetEditorTextAction == null || UpdateEditorTextAction == null) return;

            string fullText = GetEditorTextAction();
            var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var completedRegex = new Regex(@"\((?:hoàn thành|done|completed)[^)]*\)", RegexOptions.IgnoreCase);

            var headersAndNotes = new System.Collections.Generic.List<string>();
            var uncompletedTasks = new System.Collections.Generic.List<string>();
            var completedTasks = new System.Collections.Generic.List<string>();

            foreach (var line in lines)
            {
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("-", StringComparison.Ordinal))
                {
                    if (completedRegex.IsMatch(line))
                    {
                        completedTasks.Add(line);
                    }
                    else
                    {
                        uncompletedTasks.Add(line);
                    }
                }
                else
                {

                    if (!trimmed.StartsWith("# ✅ ĐÃ HOÀN THÀNH", StringComparison.OrdinalIgnoreCase) &&
                        !trimmed.StartsWith("# ✅ COMPLETED TASKS", StringComparison.OrdinalIgnoreCase))
                    {
                        headersAndNotes.Add(line);
                    }
                }
            }

            var result = new StringBuilder();

            foreach (var h in headersAndNotes)
            {
                result.AppendLine(h);
            }

            if (uncompletedTasks.Count > 0)
            {
                if (result.Length > 0 && !result.ToString().EndsWith("\r\n\r\n"))
                {
                    result.AppendLine();
                }
                foreach (var task in uncompletedTasks)
                {
                    result.AppendLine(task);
                }
            }

            if (completedTasks.Count > 0)
            {
                if (result.Length > 0 && !result.ToString().EndsWith("\r\n\r\n"))
                {
                    result.AppendLine();
                }
                string completedHeader = L10n.IsEnglish ? "# ✅ COMPLETED TASKS" : "# ✅ ĐÃ HOÀN THÀNH";
                result.AppendLine(completedHeader);
                foreach (var task in completedTasks)
                {
                    result.AppendLine(task);
                }
            }

            UpdateEditorTextAction(result.ToString().TrimEnd() + Environment.NewLine);
            CurrentTab.Content = result.ToString();
            CurrentTab.RecalculateStats();
            PerformSave();
            RefreshGhostTasks();

            StatusMessage = L10n.StatusSorted;
        }

        public void TogglePomodoroAtCurrentLine()
        {
            if (CurrentTab == null || GetEditorTextAction == null || GetCaretLineNumberAction == null) return;

            int currentLineNumber = GetCaretLineNumberAction();
            string fullText = GetEditorTextAction();
            var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (currentLineNumber < 1 || currentLineNumber > lines.Length) return;

            string line = lines[currentLineNumber - 1];
            string trimmed = line.TrimStart();

            if (!trimmed.StartsWith("-", StringComparison.Ordinal))
            {
                StatusMessage = "⚠️ Hãy đặt con trỏ ở dòng công việc bắt đầu bằng '-' để bật Pomodoro.";
                return;
            }

            if (_pomodoroService.IsRunning &&
                _pomodoroService.CurrentSession?.TabId == CurrentTab.Id &&
                _pomodoroService.CurrentSession?.LineNumber == currentLineNumber)
            {
                _pomodoroService.TogglePause();
                StatusMessage = _pomodoroService.CurrentSession.IsPaused ? "⏸ Đã tạm dừng Pomodoro" : "▶ Tiếp tục Pomodoro";
                return;
            }

            string cleanTitle = Regex.Replace(line, @"^-\s*(?:\[H\]\s*)?", "");
            cleanTitle = Regex.Replace(cleanTitle, @"\[⏳[^\]]+\]", "");
            cleanTitle = Regex.Replace(cleanTitle, @"\[🔥[^\]]+\]", "");
            cleanTitle = Regex.Replace(cleanTitle, @"@\S+", "");
            cleanTitle = cleanTitle.Trim();

            string timerTag = "[⏳ 25:00] ";
            string updatedLine;

            if (line.Contains("[⏳"))
            {
                updatedLine = Regex.Replace(line, @"\[⏳\s*[^\]]+\]\s*", timerTag);
            }
            else
            {

                if (trimmed.StartsWith("- [H]"))
                {
                    int idx = line.IndexOf("- [H]");
                    updatedLine = line.Insert(idx + 5, " " + timerTag);
                }
                else
                {
                    int idx = line.IndexOf('-');
                    updatedLine = line.Insert(idx + 1, " " + timerTag);
                }
            }

            ReplaceEditorLineAction?.Invoke(currentLineNumber, updatedLine);
            _pomodoroService.Start(CurrentTab.Id, currentLineNumber, cleanTitle, 25);
            ActivePomodoro = _pomodoroService.CurrentSession;
            StatusMessage = $"⏱ Bắt đầu Pomodoro 25p: \"{cleanTitle}\"";
            RefreshGhostTasks();
        }

        private void OnPomodoroTick(PomodoroSession session)
        {
            ActivePomodoro = session;

            if (CurrentTab != null && CurrentTab.Id == session.TabId &&
                GetEditorTextAction != null && ReplaceEditorLineAction != null)
            {
                string fullText = GetEditorTextAction();
                var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (session.LineNumber >= 1 && session.LineNumber <= lines.Length)
                {
                    string line = lines[session.LineNumber - 1];
                    if (line.Contains("[⏳"))
                    {
                        string updatedLine = Regex.Replace(line, @"\[⏳\s*[^\]]+\]", session.InlineTag);
                        ReplaceEditorLineAction(session.LineNumber, updatedLine);
                    }
                }
            }
        }

        private void OnPomodoroCompleted(PomodoroSession session)
        {
            ActivePomodoro = null;
            if (CurrentTab != null && CurrentTab.Id == session.TabId &&
                GetEditorTextAction != null && ReplaceEditorLineAction != null)
            {
                string fullText = GetEditorTextAction();
                var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (session.LineNumber >= 1 && session.LineNumber <= lines.Length)
                {
                    string line = lines[session.LineNumber - 1];

                    string tag = L10n.IsEnglish ? " (done - 25m)" : " (hoàn thành - 25p)";
                    string cleanLine = Regex.Replace(line, @"\[⏳\s*[^\]]+\]\s*", "");
                    cleanLine = Regex.Replace(cleanLine, @"\s*\((?:hoàn thành|done|completed)[^)]*\)", "");
                    string finalLine = $"{cleanLine.TrimEnd()}{tag}";
                    ReplaceEditorLineAction(session.LineNumber, finalLine);
                }
            }

            StatusMessage = L10n.StatusPomodoroCompleted;
            TriggerAutoSave();
            RefreshGhostTasks();
        }

        private void OnPomodoroFinishedEarly(PomodoroSession session, int elapsedMinutes)
        {
            ActivePomodoro = null;
            if (CurrentTab != null && CurrentTab.Id == session.TabId &&
                GetEditorTextAction != null && ReplaceEditorLineAction != null)
            {
                string fullText = GetEditorTextAction();
                var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (session.LineNumber >= 1 && session.LineNumber <= lines.Length)
                {
                    string tag = L10n.IsEnglish ? $" (done - {elapsedMinutes}m)" : $" (hoàn thành - {elapsedMinutes}p)";
                    string line = lines[session.LineNumber - 1];
                    string cleanLine = Regex.Replace(line, @"\[⏳\s*[^\]]+\]\s*", "");
                    cleanLine = Regex.Replace(cleanLine, @"\s*\((?:hoàn thành|done|completed)[^)]*\)", "");
                    string finalLine = $"{cleanLine.TrimEnd()}{tag}";
                    ReplaceEditorLineAction(session.LineNumber, finalLine);
                }
            }

            StatusMessage = L10n.FormatStatusPomodoroFinishedEarly(elapsedMinutes);
            TriggerAutoSave();
            RefreshGhostTasks();
        }

        private void OnPomodoroCancelled()
        {
            ActivePomodoro = null;
            StatusMessage = L10n.StatusPomodoroCancelled;
            RefreshGhostTasks();
        }

        [RelayCommand]
        public void StopActivePomodoro()
        {
            _pomodoroService.Cancel();
        }

        public void QuickLaunchAtCurrentLine()
        {
            if (GetEditorTextAction == null || GetCaretLineNumberAction == null) return;

            int currentLineNumber = GetCaretLineNumberAction();
            string fullText = GetEditorTextAction();
            var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (currentLineNumber < 1 || currentLineNumber > lines.Length) return;

            string line = lines[currentLineNumber - 1];
            string? result = _quickLaunchService.TryLaunchFromLine(line);

            if (result != null)
            {
                StatusMessage = result;
            }
            else
            {
                StatusMessage = "ℹ Không tìm thấy thẻ phóng @web:, @path:, @cmd: hoặc liên kết trên dòng này.";
            }
        }

        [RelayCommand]
        public void ToggleGhostMode()
        {
            IsGhostMode = !IsGhostMode;
            if (IsGhostMode)
            {
                RefreshGhostTasks();
                StatusMessage = "👻 Chế độ Ghost HUD kích hoạt (mờ 75%, luôn ở trên cùng). Bấm Ctrl+T để quay lại.";
            }
            else
            {
                StatusMessage = "🖥 Đã quay lại chế độ soạn thảo toàn màn hình.";
            }
        }

        public void RefreshGhostTasks()
        {
            GhostTasks.Clear();
            if (CurrentTab == null || string.IsNullOrWhiteSpace(CurrentTab.Content)) return;

            var lines = CurrentTab.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var completedRegex = new Regex(@"\((?:hoàn thành|done|completed)[^)]*\)", RegexOptions.IgnoreCase);
            var streakRegex = new Regex(@"\[🔥\s*[^\]]+\]", RegexOptions.IgnoreCase);
            var timerRegex = new Regex(@"\[⏳\s*[^\]]+\]", RegexOptions.IgnoreCase);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.TrimStart();

                if (trimmed.StartsWith("-", StringComparison.Ordinal))
                {
                    bool isCompleted = completedRegex.IsMatch(line);

                    if (!isCompleted)
                    {
                        string display = Regex.Replace(trimmed, @"^-\s*(?:\[H\]\s*)?", "");
                        display = timerRegex.Replace(display, "");
                        display = streakRegex.Replace(display, "");
                        display = display.Trim();

                        string streak = string.Empty;
                        var streakMatch = streakRegex.Match(line);
                        if (streakMatch.Success) streak = streakMatch.Value;

                        string timerText = string.Empty;
                        bool hasTimer = false;
                        var timerMatch = timerRegex.Match(line);
                        if (timerMatch.Success)
                        {
                            hasTimer = true;
                            timerText = timerMatch.Value;
                        }

                        GhostTasks.Add(new GhostTaskItem
                        {
                            TabId = CurrentTab.Id,
                            TabTitle = CurrentTab.Title,
                            LineNumber = i + 1,
                            RawText = line,
                            DisplayText = display,
                            IsCompleted = false,
                            IsHabit = trimmed.Contains("[H]"),
                            Streak = streak,
                            HasActiveTimer = hasTimer,
                            TimerDisplay = timerText
                        });
                    }
                }
            }
        }

        [RelayCommand]
        public void CompleteGhostTask(GhostTaskItem? item)
        {
            if (item == null || CurrentTab == null || GetEditorTextAction == null || ReplaceEditorLineAction == null) return;

            string fullText = GetEditorTextAction();
            var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (item.LineNumber >= 1 && item.LineNumber <= lines.Length)
            {
                string line = lines[item.LineNumber - 1];
                string tag = L10n.IsEnglish ? " (done)" : " (hoàn thành)";
                string newLine = $"{line.TrimEnd()}{tag}";
                ReplaceEditorLineAction(item.LineNumber, newLine);
                TriggerAutoSave();
                RefreshGhostTasks();
                StatusMessage = L10n.StatusCompleted;
            }
        }
    }
}
