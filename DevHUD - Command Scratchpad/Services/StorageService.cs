using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DevHUD___Command_Scratchpad.Models;

namespace DevHUD___Command_Scratchpad.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _storageDir;
        private readonly string _storageFile;
        private readonly string _snapshotsDir;
        private readonly JsonSerializerOptions _jsonOptions;

        public StorageService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _storageDir = Path.Combine(appData, "DevHUD");
            _storageFile = Path.Combine(_storageDir, "scratchpad_data.json");
            _snapshotsDir = Path.Combine(_storageDir, "snapshots");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            EnsureDirectories();
        }

        private void EnsureDirectories()
        {
            if (!Directory.Exists(_storageDir))
            {
                Directory.CreateDirectory(_storageDir);
            }
            if (!Directory.Exists(_snapshotsDir))
            {
                Directory.CreateDirectory(_snapshotsDir);
            }
        }

        public AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_storageFile))
                {
                    string json = File.ReadAllText(_storageFile, Encoding.UTF8);
                    var config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions);
                    if (config != null && config.Tabs != null && config.Tabs.Count > 0)
                    {
                        foreach (var tab in config.Tabs)
                        {
                            tab.RecalculateStats();
                        }
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StorageService] LoadConfig error: {ex.Message}");
            }

            var defaultConfig = new AppConfig
            {
                LastActiveDate = DateTime.Today,
                Tabs = GetSeedData()
            };
            if (defaultConfig.Tabs.Count > 0)
            {
                defaultConfig.ActiveTabId = defaultConfig.Tabs[0].Id;
            }
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        public void SaveConfig(AppConfig config)
        {
            try
            {
                EnsureDirectories();
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_storageFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StorageService] SaveConfig error: {ex.Message}");
            }
        }

        public void CreateDailySnapshot(AppConfig config)
        {
            try
            {
                EnsureDirectories();
                string snapshotPath = Path.Combine(_snapshotsDir, $"snapshot_{DateTime.Today:yyyyMMdd}.json");
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(snapshotPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StorageService] CreateDailySnapshot error: {ex.Message}");
            }
        }

        public bool ProcessDailyHabitRespawn(AppConfig config)
        {
            var today = DateTime.Today;
            var lastActive = config.LastActiveDate.Date;

            if (today <= lastActive)
            {
                return false;
            }

            int daysPassed = (today - lastActive).Days;
            bool anyHabitUpdated = false;

            CreateDailySnapshot(config);

            var streakRegex = new Regex(@"\[🔥\s*(\d+)\s*(?:ngày|ngay|days|day)?\]", RegexOptions.IgnoreCase);
            var completedRegex = new Regex(@"\s*\((?:hoàn thành|done|completed)[^)]*\)", RegexOptions.IgnoreCase);

            foreach (var tab in config.Tabs)
            {
                if (string.IsNullOrWhiteSpace(tab.Content)) continue;

                var lines = tab.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                bool tabModified = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string trimmed = line.TrimStart();

                    if (trimmed.StartsWith("-", StringComparison.Ordinal) && trimmed.Contains("[H]"))
                    {
                        bool wasCompleted = completedRegex.IsMatch(line);

                        if (wasCompleted)
                        {
                            anyHabitUpdated = true;
                            tabModified = true;

                            int currentStreak = 1;
                            var match = streakRegex.Match(line);
                            if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedStreak))
                            {
                                currentStreak = parsedStreak;
                            }

                            int newStreak = (daysPassed == 1) ? (currentStreak + 1) : 1;

                            string cleanLine = completedRegex.Replace(line, "");

                            string unit = LocalizationService.Instance.IsEnglish ? "days" : "ngày";

                            if (streakRegex.IsMatch(cleanLine))
                            {
                                cleanLine = streakRegex.Replace(cleanLine, $"[🔥 {newStreak} {unit}]");
                            }
                            else
                            {
                                cleanLine = $"{cleanLine.TrimEnd()} [🔥 {newStreak} {unit}]";
                            }

                            lines[i] = cleanLine;
                        }
                    }
                }

                if (tabModified)
                {
                    tab.Content = string.Join(Environment.NewLine, lines);
                    tab.RecalculateStats();
                }
            }

            config.LastActiveDate = today;
            SaveConfig(config);
            return anyHabitUpdated;
        }

        public List<TabModel> GetSeedData()
        {
            var tab1Content = new StringBuilder();
            tab1Content.AppendLine("# ⚡ CÔNG VIỆC HÔM NAY (ZERO-FRICTION SCRATCHPAD)");
            tab1Content.AppendLine("- [H] làm duolingo (hoàn thành) [🔥 4 ngày]");
            tab1Content.AppendLine("- [H] viết nhật ký");
            tab1Content.AppendLine("- làm canva slide thuyết trình pttkht @web:https://www.canva.com");
            tab1Content.AppendLine("- kiểm tra cái môn pttkht, và ghi chép bài");
            tab1Content.AppendLine("- phân công việc nhóm đồ án @path:D:\\");
            tab1Content.AppendLine("- thể dục cardio 15 phút");
            tab1Content.AppendLine("");
            tab1Content.AppendLine("# 📌 Phím tắt thần tốc:");
            tab1Content.AppendLine("- [Ctrl + Enter]: Toggle hoàn thành việc ngay tại dòng");
            tab1Content.AppendLine("- [F5]: Kích hoạt Inline Micro-Pomodoro đếm giờ trực tiếp");
            tab1Content.AppendLine("- [Ctrl + Space]: Phóng nhanh Web/Thư mục/Lệnh tại dòng");
            tab1Content.AppendLine("- [Ctrl + Shift + S]: Đẩy tất cả việc đã xong xuống đáy");
            tab1Content.AppendLine("- [Ctrl + T]: Bật chế độ Ghost HUD bóng ma mờ 70% ghim góc màn hình");
            tab1Content.AppendLine("- [Alt + Q]: Ẩn/Hiện cửa sổ từ bất cứ đâu trên Windows");

            var tab2Content = new StringBuilder();
            tab2Content.AppendLine("# 🧠 THUẬT TOÁN & PHỎNG VẤN LEETCODE");
            tab2Content.AppendLine("- Ôn tập kỹ thuật Two Pointers & Sliding Window @web:https://leetcode.com/problemset/all/");
            tab2Content.AppendLine("- Giải bài 3. Longest Substring Without Repeating Characters (hoàn thành - 25p)");
            tab2Content.AppendLine("- Luyện tập đồ thị BFS/DFS tìm đường đi ngắn nhất");
            tab2Content.AppendLine("- Viết note tóm tắt Dynamic Programming: 0/1 Knapsack");
            tab2Content.AppendLine("- Đọc tài liệu System Design Primer @web:https://github.com/donnemartin/system-design-primer");

            var tab3Content = new StringBuilder();
            tab3Content.AppendLine("# 🚀 ĐỒ ÁN HK1 - NĂM 3 (DESTRUCTION & REBUILD)");
            tab3Content.AppendLine("- Họp nhóm thống nhất Database Schema & Architecture");
            tab3Content.AppendLine("- Viết tài liệu đặc tả yêu cầu SRS Chương 3 (hoàn thành)");
            tab3Content.AppendLine("- Thiết kế lại ERD và quan hệ các bảng cơ sở dữ liệu");
            tab3Content.AppendLine("- Cấu hình CI/CD GitHub Actions build & test tự động");
            tab3Content.AppendLine("- Triển khai Authentication Service với JWT & Refresh Token");
            tab3Content.AppendLine("- Mở thư mục mã nguồn đồ án @path:D:\\2 Personal Activities\\Code Folder\\hk1 - nam 3");

            return new List<TabModel>
            {
                new TabModel("Hôm nay", tab1Content.ToString()),
                new TabModel("Thuật toán", tab2Content.ToString()),
                new TabModel("Đồ án HK1", tab3Content.ToString())
            };
        }
    }
}
