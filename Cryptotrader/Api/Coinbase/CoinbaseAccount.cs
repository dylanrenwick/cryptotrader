using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseAccount
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("balance")]
        public float Balance { get; set; }
        [JsonPropertyName("hold")]
        public float HeldBalance { get; set; }
        [JsonPropertyName("available")]
        public float AvailableBalance { get; set; }
        [JsonPropertyName("profile_id")]
        public string ProfileID { get; set; }
        [JsonPropertyName("trading_enabled")]
        public bool IsTradingEnabled { get; set; }

        public bool IsInUse => Balance != 0 || HeldBalance != 0 || AvailableBalance != 0;
    }
}
