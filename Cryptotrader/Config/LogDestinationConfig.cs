using System.Text.Json.Serialization;

using Cryptotrader.Logging;

namespace Cryptotrader.Config
{
    public class LogDestinationConfig
    {
        public enum LogDestinationType
        {
            File,
            Console
        }

        [JsonPropertyName("type")]
        public LogDestinationType DestinationType { get; set; }
        [JsonPropertyName("level")]
        public LogLevel LogLevel { get; set; }
        [JsonPropertyName("path")]
        public string FilePath { get; set; }

        public ILogDestination GetDestination()
        {
            switch (DestinationType)
            {
                case LogDestinationType.File:
                    return new FileLogDestination(FilePath, LogLevel);
                case LogDestinationType.Console:
                    return new ConsoleLogDestination(LogLevel);
                default:
                    throw new Exception($"Invalid DestinationType: {DestinationType}");
            }
        }
    }
}
