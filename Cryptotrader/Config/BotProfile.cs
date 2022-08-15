using System.Text.Json.Serialization;

using Cryptotrader.State;

namespace Cryptotrader.Config
{
    public class BotProfile
    {
        [JsonPropertyName("last_buy_price")]
        public decimal? LastBuyPrice { get; set; }
        [JsonPropertyName("last_sell_price")]
        public decimal? LastSellPrice { get; set; }

        [JsonPropertyName("cash")]
        public string Cash { get; set; }
        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonIgnore]
        public string Product => $"{Coin}-{Cash}";

        [JsonPropertyName("liquid_value")]
        public decimal LiquidValue { get; set; }

        [JsonPropertyName("gain_threshold")]
        public decimal GainThreshold { get; set; }
        [JsonPropertyName("loss_threshold")]
        public decimal LossThreshold { get; set; }

        [JsonPropertyName("rise_end_threshold")]
        public decimal RiseEndThreshold { get; set; }
        [JsonPropertyName("fall_end_threshold")]
        public decimal FallEndThreshold { get; set; }


        [JsonPropertyName("initial_state")]
        public BotState InitialState { get; set; }
    }
}
