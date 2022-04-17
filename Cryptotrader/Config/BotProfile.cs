using System.Text.Json.Serialization;

using Cryptotrader.State;

namespace Cryptotrader.Config
{
    public class BotProfile
    {
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
