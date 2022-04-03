using System;
using System.Collections.Generic;
using System.Linq;

namespace Cryptotrader.Logging
{
    public class LogMessage
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Label { get; set; }

        public override string ToString()
        {
            string[] lines = Message.Split('\n');
            return string.Join(
                "\n",
                lines.Select(
                    (l, i) =>
                        i == 0
                        ? FormatHeadLine(l)
                        : FormatMultiline(l)
                )
            ) + "\n";
        }

        private string FormatHeadLine(string line)
        {
            return $"{FormattedLevel}\t{FormattedTimestamp} {FormattedLabel}> {line}";
        }
        private string FormatMultiline(string line)
        {
            return $"\t{new string(' ', FormattedTimestamp.Length)} {FormattedLabel}> {line}";
        }

        private string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH-mm-ss.ffff");
        private string FormattedLevel
        {
            get
            {
                string formattedLevel = Level.ToString().PadRight(5).Substring(0, 5).ToUpper();
                if (levelColors.TryGetValue(Level, out ConsoleColor color))
                    return ColorText(formattedLevel, color);
                else return formattedLevel;
            }
        }
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
