using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace DevHUD___Command_Scratchpad.Services
{
    public class QuickLaunchService : IQuickLaunchService
    {
        private static readonly Regex WebTagRegex = new(@"@web:(\S+)", RegexOptions.IgnoreCase);
        private static readonly Regex PathTagRegex = new(@"@path:([^\s]+(?:[ \t]+[^\s@]+)*)", RegexOptions.IgnoreCase);
        private static readonly Regex CmdTagRegex = new(@"@(?:cmd|run):([^\r\n]+)", RegexOptions.IgnoreCase);
        private static readonly Regex BareUrlRegex = new(@"(https?://[^\s]+)", RegexOptions.IgnoreCase);
        private static readonly Regex BarePathRegex = new(@"([a-zA-Z]:\\[^\s<>\:""/\\|?*]+(?:\\[^\s<>\:""/\\|?*]+)*)", RegexOptions.IgnoreCase);

        public string? TryLaunchFromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var webMatch = WebTagRegex.Match(line);
            if (webMatch.Success)
            {
                string url = webMatch.Groups[1].Value.Trim();
                return LaunchUrl(url);
            }

            var pathMatch = PathTagRegex.Match(line);
            if (pathMatch.Success)
            {
                string path = pathMatch.Groups[1].Value.Trim('"', '\'', ' ');
                return LaunchPath(path);
            }

            var cmdMatch = CmdTagRegex.Match(line);
            if (cmdMatch.Success)
            {
                string cmd = cmdMatch.Groups[1].Value.Trim();
                return LaunchCommand(cmd);
            }

            var bareUrlMatch = BareUrlRegex.Match(line);
            if (bareUrlMatch.Success)
            {
                string url = bareUrlMatch.Groups[1].Value.Trim();
                return LaunchUrl(url);
            }

            var barePathMatch = BarePathRegex.Match(line);
            if (barePathMatch.Success)
            {
                string path = barePathMatch.Groups[1].Value.Trim('"', '\'', ' ');
                if (Directory.Exists(path) || File.Exists(path))
                {
                    return LaunchPath(path);
                }
            }

            return null;
        }

        private string? LaunchUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                return $"🚀 Phóng trình duyệt: {url}";
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi mở URL: {ex.Message}";
            }
        }

        private string? LaunchPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true
                    });
                    return $"📁 Đã mở thư mục: {path}";
                }
                else if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    return $"📄 Đã mở tệp: {Path.GetFileName(path)}";
                }
                else
                {

                    var parent = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{parent}\"",
                            UseShellExecute = true
                        });
                        return $"📁 Đã mở thư mục cha: {parent}";
                    }

                    return $"⚠️ Đường dẫn không tồn tại: {path}";
                }
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi mở đường dẫn: {ex.Message}";
            }
        }

        private string? LaunchCommand(string command)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"{command}\"",
                    UseShellExecute = true
                });
                return $"⚡ Đang thực thi lệnh: {command}";
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi chạy lệnh: {ex.Message}";
            }
        }
    }
}
