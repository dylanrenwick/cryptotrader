using System;
using System.Text.Json.Serialization;

namespace Cryptotrader.Config
{
    public class BotConfig
    {
        [JsonPropertyName("interval")]
        public int UpdateInterval { get; set; }\
    }
}
