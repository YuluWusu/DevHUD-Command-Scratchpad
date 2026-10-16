using System;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHUD___Command_Scratchpad.Models
{
    public partial class TabModel : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString("N");

        [ObservableProperty]
        private string _title = "Ghi chú mới";

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private int _totalTasks;

        [ObservableProperty]
        private int _completedTasks;

        [ObservableProperty]
        private double _completionPercentage;

        [ObservableProperty]
        private DateTime _lastModified = DateTime.Now;

        public TabModel()
        {
        }

        public TabModel(string title, string content)
        {
            _title = title;
            _content = content;
            RecalculateStats();
        }

        partial void OnContentChanged(string value)
        {
            LastModified = DateTime.Now;
            RecalculateStats();
        }

        public void RecalculateStats()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                TotalTasks = 0;
                CompletedTasks = 0;
                CompletionPercentage = 0;
                return;
            }

            var lines = Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int total = 0;
            int completed = 0;

            foreach (var rawLine in lines)
            {
                var trimmed = rawLine.TrimStart();
                if (trimmed.StartsWith("- ", StringComparison.Ordinal))
                {
                    total++;
                    if (trimmed.Contains("(hoàn thành", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.Contains("(done", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.Contains("(completed", StringComparison.OrdinalIgnoreCase))
                    {
                        completed++;
                    }
                }
            }

            TotalTasks = total;
            CompletedTasks = completed;
            CompletionPercentage = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;
        }
    }
}
