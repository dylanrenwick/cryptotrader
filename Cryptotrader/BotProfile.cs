using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Cryptotrader.State;

namespace Cryptotrader
{
    public class BotProfile
    {
        public decimal LiquidValue { get; set; }

        public decimal GainThreshold { get; set; }
        public decimal LossThreshold { get; set; }

        public decimal RiseEndThreshold { get; set; }
        public decimal FallEndThreshold { get; set; }

        public bool BuyOnStartup { get; set; }

        public decimal LastSoldPrice { get; set; }
        public decimal LastBoughtPrice { get; set; }

        [JsonConverter(typeof(BotStateJsonConverter))]
        public BotStateBehavior InitialState { get; set; }
    }
}
