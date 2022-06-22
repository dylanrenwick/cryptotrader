using System.Text;

namespace Cryptotrader.Logging
{
    public class LogMessage
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public LogLevel Level { get; init; }
        public string Message { get; init; }
        public string Label { get; init; }

        public override string ToString()
            => ToString(true);
        public string ToString(bool useANSICodes)
        {
            string[] lines = Message.Split('\n');
            StringBuilder sb = new();
            sb.AppendLine(FormatHeadLine(lines[0], useANSICodes));
            for (int i = 1; i < lines.Length; i++)
            {
                sb.AppendLine(FormatMultiline(lines[i]));
            }
            return sb.ToString();
        }

        private string FormatHeadLine(string line, bool useANSICodes)
        {
            StringBuilder sb = new();
            if (useANSICodes) sb.Append(ResetCode);
            sb.Append(FormattedTimestamp);
            sb.Append(FormatLevel(useANSICodes));
            sb.Append(FormattedLabel);
            sb.Append("> ");
            sb.Append(line);
            return sb.ToString();
        }
        private string FormatMultiline(string line)
        {
            return $"\t{new string(' ', FormattedTimestamp.Length)} {FormattedLabel}> {line}";
        }

        private string FormatLevel(bool useANSICodes)
        {
            string formattedLevel = Level.ToString().PadLeft(6).ToUpper();
            if (useANSICodes && levelColors.TryGetValue(Level, out ConsoleColor color))
                return ColorText(formattedLevel, color);
            else return formattedLevel;
        }

        private string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH-mm-ss.ffff");
        private string FormattedLabel => $"|{Label.PadRight(3, ' ').Substring(0, 3).ToUpper()}|";

        private static readonly Dictionary<LogLevel, ConsoleColor> levelColors = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.Crit, ConsoleColor.Red },
            { LogLevel.Error, ConsoleColor.DarkRed },
            { LogLevel.Warn, ConsoleColor.Yellow },
            { LogLevel.Alert, ConsoleColor.White },
            { LogLevel.Debug, ConsoleColor.Cyan }
        };

        private static readonly Dictionary<ConsoleColor, string> colorCodes = new Dictionary<ConsoleColor, string>
        {
            { ConsoleColor.Black, "30" },
            { ConsoleColor.DarkRed, "31" },
            { ConsoleColor.DarkGreen, "32" },
            { ConsoleColor.DarkYellow, "33" },
            { ConsoleColor.DarkBlue, "34" },
            { ConsoleColor.DarkMagenta, "35" },
            { ConsoleColor.DarkCyan, "36" },
            { ConsoleColor.Gray, "37" },
            { ConsoleColor.Red, "31;1" },
            { ConsoleColor.Green, "32;1" },
            { ConsoleColor.Yellow, "33;1" },
            { ConsoleColor.Blue, "34;1" },
            { ConsoleColor.Magenta, "35;1" },
            { ConsoleColor.Cyan, "36;1" },
            { ConsoleColor.White, "37;1" },
        };

        private string ColorText(string text, ConsoleColor color)
        {
            return $"{GetColorCode(color)}{text}{ResetCode}";
        }

        private string ResetCode => GetColorCode(0);
        private string GetColorCode(ConsoleColor color) => GetColorCode(colorCodes[color]);
        private string GetColorCode(string code) => $"\u001b[{code}m";
    }
}
