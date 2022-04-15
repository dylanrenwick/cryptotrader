using System.Text.Json.Serialization;

using Cryptotrader.State;

namespace Cryptotrader.Config
{
    public class BotConfig
    {
        [JsonPropertyName("interval")]
        public int UpdateInterval { get; set; }
        [JsonPropertyName("initial_state")]
        public BotState InitialState { get; set; }

        [JsonPropertyName("drop_before_buy")]
        public decimal DropBeforeBuy { get; set; }
        [JsonPropertyName("rebound_before_buy")]
        public decimal ReboundBeforeBuy { get; set; }

        [JsonPropertyName("drop_before_sell")]
        public decimal GainBeforeSell { get; set; }
        [JsonPropertyName("drop_before_sell")]
        public decimal ReboundBeforeSell { get; set; }

        [JsonPropertyName("buy_amount")]
        public decimal AmountToBuy { get; set; }
    }
}
