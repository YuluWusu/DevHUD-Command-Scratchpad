using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace DevHUD___Command_Scratchpad.Editor
{
    public class TacticalColorizer : DocumentColorizingTransformer
    {

        private static readonly SolidColorBrush HeaderBrush = new(Color.FromRgb(0x58, 0xA6, 0xFF));
        private static readonly SolidColorBrush BulletBrush = new(Color.FromRgb(0x00, 0xF0, 0xFF));
        private static readonly SolidColorBrush HabitBrush = new(Color.FromRgb(0xD2, 0xA8, 0xFF));
        private static readonly SolidColorBrush CompletedTextBrush = new(Color.FromRgb(0x6E, 0x76, 0x81));
        private static readonly SolidColorBrush CompletedBadgeBrush = new(Color.FromRgb(0x3F, 0xB9, 0x50));
        private static readonly SolidColorBrush TimerBrush = new(Color.FromRgb(0xFB, 0xBF, 0x24));
        private static readonly SolidColorBrush TimerBgBrush = new(Color.FromArgb(120, 0x78, 0x35, 0x0F));
        private static readonly SolidColorBrush StreakBrush = new(Color.FromRgb(0xFF, 0x7B, 0x72));
        private static readonly SolidColorBrush WebTagBrush = new(Color.FromRgb(0x38, 0xBD, 0xF8));
        private static readonly SolidColorBrush PathTagBrush = new(Color.FromRgb(0xC0, 0x84, 0xFC));
        private static readonly SolidColorBrush CmdTagBrush = new(Color.FromRgb(0x4A, 0xDE, 0x80));
        private static readonly SolidColorBrush NormalTextBrush = new(Color.FromRgb(0xE6, 0xED, 0xF3));

        static TacticalColorizer()
        {
            HeaderBrush.Freeze();
            BulletBrush.Freeze();
            HabitBrush.Freeze();
            CompletedTextBrush.Freeze();
            CompletedBadgeBrush.Freeze();
            TimerBrush.Freeze();
            TimerBgBrush.Freeze();
            StreakBrush.Freeze();
            WebTagBrush.Freeze();
            PathTagBrush.Freeze();
            CmdTagBrush.Freeze();
            NormalTextBrush.Freeze();
        }

        private static readonly Regex TimerRegex = new(@"\[⏳\s*[^\]]+\]", RegexOptions.Compiled);
        private static readonly Regex StreakRegex = new(@"\[🔥\s*[^\]]+\]", RegexOptions.Compiled);
        private static readonly Regex CompletedRegex = new(@"\((?:hoàn thành|done|completed)[^)]*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WebTagRegex = new(@"@web:\S+|https?://\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PathTagRegex = new(@"@path:[^\s]+|[a-zA-Z]:\\[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CmdTagRegex = new(@"@(?:cmd|run):[^\r\n]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);

            if (string.IsNullOrWhiteSpace(text)) return;

            string trimmed = text.TrimStart();
            bool isCompleted = CompletedRegex.IsMatch(text);

            if (trimmed.StartsWith("#", StringComparison.Ordinal))
            {
                ChangeLinePart(lineStartOffset, line.EndOffset, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(HeaderBrush);
                });
                return;
            }

            if (isCompleted)
            {
                ChangeLinePart(lineStartOffset, line.EndOffset, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(CompletedTextBrush);
                    element.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
                });

                foreach (Match m in CompletedRegex.Matches(text))
                {
                    int start = lineStartOffset + m.Index;
                    int end = start + m.Length;
                    ChangeLinePart(start, end, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(CompletedBadgeBrush);
                        element.TextRunProperties.SetTextDecorations(new TextDecorationCollection());
                    });
                }
            }
            else if (trimmed.StartsWith("-", StringComparison.Ordinal))
            {

                ChangeLinePart(lineStartOffset, line.EndOffset, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(NormalTextBrush);
                });

                int bulletIndex = text.IndexOf('-');
                if (bulletIndex >= 0)
                {
                    int bulletStart = lineStartOffset + bulletIndex;
                    int bulletEnd = Math.Min(bulletStart + 2, line.EndOffset);
                    ChangeLinePart(bulletStart, bulletEnd, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(BulletBrush);
                    });
                }
            }

            int habitIdx = text.IndexOf("[H]", StringComparison.Ordinal);
            if (habitIdx >= 0)
            {
                int start = lineStartOffset + habitIdx;
                int end = start + 3;
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(HabitBrush);
                });
            }

            foreach (Match m in TimerRegex.Matches(text))
            {
                int start = lineStartOffset + m.Index;
                int end = start + m.Length;
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TimerBrush);
                    element.BackgroundBrush = TimerBgBrush;
                    element.TextRunProperties.SetTextDecorations(new TextDecorationCollection());
                });
            }

            foreach (Match m in StreakRegex.Matches(text))
            {
                int start = lineStartOffset + m.Index;
                int end = start + m.Length;
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(StreakBrush);
                    element.TextRunProperties.SetTextDecorations(new TextDecorationCollection());
                });
            }

            foreach (Match m in WebTagRegex.Matches(text))
            {
                int start = lineStartOffset + m.Index;
                int end = start + m.Length;
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(WebTagBrush);
                    element.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
                });
            }

            foreach (Match m in PathTagRegex.Matches(text))
            {
                int start = lineStartOffset + m.Index;
                int end = start + m.Length;
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(PathTagBrush);
                });
            }

            foreach (Match m in CmdTagRegex.Matches(text))
            {
                int start = lineStartOffset + m.Index;
                int end = start + m.Length;
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(CmdTagBrush);
                });
            }
        }
    }
}
