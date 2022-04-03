using System.Text.Json.Serialization;

namespace Cryptotrader.Config
{
    public class ConfigState
    {
        [JsonPropertyName("loggers")]
        public LogDestinationConfig[] DestinationConfigs { get; set; }
        [JsonPropertyName("api")]
        public ApiConfig ApiConfig { get; set; }
    }
}
