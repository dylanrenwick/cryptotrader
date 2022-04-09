using System;
using System.Text.Json.Serialization;

namespace Cryptotrader.Config
{
    public class BotConfig
    {
        [JsonPropertyName("interval")]
        public int UpdateInterval { get; set; }
        [JsonPropertyName("initial_state")]
        public BotState InitialState { get; set; }
    }
}
