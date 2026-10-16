using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using DevHUD___Command_Scratchpad.Editor;
using DevHUD___Command_Scratchpad.Models;
using DevHUD___Command_Scratchpad.Services;
using DevHUD___Command_Scratchpad.ViewModels;

namespace DevHUD___Command_Scratchpad
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly IHotkeyService _hotkeyService;
        private bool _isUpdatingTextInternally;
        private double _prevLeft;
        private double _prevTop;
        private double _prevWidth;
        private double _prevHeight;

        public MainWindow()
        {
            InitializeComponent();

            var storageService = new StorageService();
            var pomodoroService = new PomodoroService();
            var quickLaunchService = new QuickLaunchService();
            _hotkeyService = new HotkeyService();

            _viewModel = new MainViewModel(storageService, pomodoroService, quickLaunchService);
            DataContext = _viewModel;

            InitializeEditor();
            WireViewModelActions();

            _hotkeyService.GlobalHotkeyTriggered += OnGlobalHotkeyTriggered;
            _hotkeyService.Register(this);

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            if (_viewModel.CurrentTab != null)
            {
                MainEditor.Text = _viewModel.CurrentTab.Content;
            }
        }

        private void InitializeEditor()
        {
            MainEditor.Options.EnableHyperlinks = false;
            MainEditor.Options.ConvertTabsToSpaces = true;
            MainEditor.Options.IndentationSize = 2;
            MainEditor.Options.AllowScrollBelowDocument = true;

            MainEditor.TextArea.TextView.LineTransformers.Add(new TacticalColorizer());
        }

        private void WireViewModelActions()
        {
            _viewModel.GetEditorTextAction = () => MainEditor.Text;
            _viewModel.GetCaretLineNumberAction = () => MainEditor.TextArea.Caret.Line;

            _viewModel.UpdateEditorTextAction = (newText) =>
            {
                _isUpdatingTextInternally = true;
                int caretOffset = Math.Min(MainEditor.CaretOffset, newText.Length);
                MainEditor.Text = newText;
                MainEditor.CaretOffset = caretOffset;
                _isUpdatingTextInternally = false;
            };

            _viewModel.ReplaceEditorLineAction = (lineNumber, newLineText) =>
            {
                var doc = MainEditor.Document;
                if (lineNumber >= 1 && lineNumber <= doc.LineCount)
                {
                    _isUpdatingTextInternally = true;
                    var line = doc.GetLineByNumber(lineNumber);
                    int caretOffset = MainEditor.CaretOffset;

                    doc.Replace(line.Offset, line.Length, newLineText);

                    int newLineLength = newLineText.Length;
                    if (caretOffset >= line.Offset && caretOffset <= line.Offset + line.Length)
                    {
                        MainEditor.CaretOffset = Math.Min(caretOffset, line.Offset + newLineLength);
                    }

                    _isUpdatingTextInternally = false;

                    if (_viewModel.CurrentTab != null)
                    {
                        _viewModel.CurrentTab.Content = MainEditor.Text;
                        _viewModel.CurrentTab.RecalculateStats();
                    }
                }
            };
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentTab))
            {
                if (_viewModel.CurrentTab != null && !_isUpdatingTextInternally)
                {
                    _isUpdatingTextInternally = true;
                    MainEditor.Text = _viewModel.CurrentTab.Content ?? string.Empty;
                    MainEditor.CaretOffset = 0;
                    _isUpdatingTextInternally = false;
                }
            }
            else if (e.PropertyName == nameof(MainViewModel.IsGhostMode))
            {
                ApplyGhostModeWindowTransform(_viewModel.IsGhostMode);
            }
        }

        private void ApplyGhostModeWindowTransform(bool isGhost)
        {
            if (isGhost)
            {

                _prevLeft = Left;
                _prevTop = Top;
                _prevWidth = Width;
                _prevHeight = Height;

                var workArea = SystemParameters.WorkArea;
                Width = 420;
                Height = 540;
                Left = workArea.Right - Width - 24;
                Top = workArea.Top + 24;

                Topmost = true;
                _viewModel.GhostOpacity = 0.75;
            }
            else
            {

                Topmost = false;
                _viewModel.GhostOpacity = 1.0;

                if (_prevWidth > 0 && _prevHeight > 0)
                {
                    Width = _prevWidth;
                    Height = _prevHeight;
                    Left = _prevLeft;
                    Top = _prevTop;
                }
                else
                {
                    Width = 1020;
                    Height = 680;
                }
            }
        }

        private void MainEditor_TextChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingTextInternally) return;

            if (_viewModel.CurrentTab != null)
            {
                _viewModel.CurrentTab.Content = MainEditor.Text;
                _viewModel.CurrentTab.RecalculateStats();
                _viewModel.TriggerAutoSave();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool isAlt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

            if (isCtrl && !isShift && !isAlt && (e.Key == Key.Return || e.Key == Key.Enter))
            {
                _viewModel.ToggleCompletionAtCurrentLine();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F5 && !isCtrl && !isShift && !isAlt)
            {
                _viewModel.TogglePomodoroAtCurrentLine();
                e.Handled = true;
                return;
            }

            if (isCtrl && !isShift && !isAlt && e.Key == Key.Space)
            {
                _viewModel.QuickLaunchAtCurrentLine();
                e.Handled = true;
                return;
            }

            if (isCtrl && isShift && e.Key == Key.S)
            {
                _viewModel.SortCurrentTabTasks();
                e.Handled = true;
                return;
            }

            if (isCtrl && !isShift && !isAlt && e.Key == Key.T)
            {
                _viewModel.ToggleGhostMode();
                e.Handled = true;
                return;
            }

            if (isCtrl && !isShift && !isAlt && e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                int index = e.Key - Key.D1;
                _viewModel.SelectTabByIndex(index);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (_viewModel.IsHelpVisible)
                {
                    _viewModel.CloseHelp();
                    e.Handled = true;
                    return;
                }
            }

            if (isCtrl && !isShift && !isAlt && e.Key == Key.H)
            {
                _viewModel.ToggleHelp();
                e.Handled = true;
                return;
            }

            if (isCtrl && !isShift && !isAlt && e.Key == Key.L)
            {
                _viewModel.ToggleLanguage();
                e.Handled = true;
                return;
            }

            if (isCtrl && !isShift && !isAlt && e.Key == Key.N)
            {
                _viewModel.AddTab();
                e.Handled = true;
                return;
            }

            if (isCtrl && !isShift && !isAlt && e.Key == Key.Tab)
            {
                if (_viewModel.Tabs.Count > 1 && _viewModel.CurrentTab != null)
                {
                    int currentIndex = _viewModel.Tabs.IndexOf(_viewModel.CurrentTab);
                    int nextIndex = (currentIndex + 1) % _viewModel.Tabs.Count;
                    _viewModel.CurrentTab = _viewModel.Tabs[nextIndex];
                }
                e.Handled = true;
                return;
            }
        }

        private void OnGlobalHotkeyTriggered()
        {
            Dispatcher.Invoke(() =>
            {
                if (Visibility == Visibility.Visible && IsActive && WindowState != WindowState.Minimized)
                {
                    Hide();
                }
                else
                {
                    Show();
                    WindowState = WindowState.Normal;
                    Activate();
                    Focus();
                }
            });
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeOrRestore();
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeOrRestore();
        }

        private void MaximizeOrRestore()
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

            _viewModel.PerformSave();
            Application.Current.Shutdown();
        }

        private void TabItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TabModel tab)
            {
                _viewModel.CurrentTab = tab;
            }
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is TabModel tab)
            {
                _viewModel.CloseTab(tab);
            }
        }

        private void TabTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.PerformSave();
        }

        private void CompleteGhostTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is GhostTaskItem item)
            {
                _viewModel.CompleteGhostTask(item);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _viewModel.PerformSave();
            _hotkeyService.Dispose();
            base.OnClosing(e);
        }
    }
}
