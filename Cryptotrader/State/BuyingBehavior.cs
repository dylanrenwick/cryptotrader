﻿using System.Text.Json;

using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class BuyingBehavior : BotStateBehavior
    {
        public override BotState State => Buying;

        private decimal lowestPrice;

        public BuyingBehavior()
        {
            lowestPrice = decimal.MaxValue;
        }
        public BuyingBehavior(decimal low)
        {
            lowestPrice = low;
        }

        public override void Update(ICryptoExchange api, BotProfile profile)
        {
            if (api.CurrentBuyPrice < lowestPrice)
            {
                log.Debug($"New lowest price of {api.CurrentBuyPrice}, previous lowest was {lowestPrice}");
                lowestPrice = api.CurrentBuyPrice;
            }
            else if (api.CurrentBuyPrice > lowestPrice)
            {
                log.Debug($"Price of {api.CurrentBuyPrice} is higher than low of {lowestPrice}, checking rebound threshold");
                decimal reboundAmount = GetRebound(api);
                log.Debug($"Price has rebounded by {reboundAmount * 100}% Threshold is {profile.FallEndThreshold * 100}%");
                if (reboundAmount >= profile.FallEndThreshold)
                {
                    log.Info($"Rebound amount of {reboundAmount * 100}% is greater than threshold of {profile.FallEndThreshold * 100}%");
                    log.Info("Price has stopped falling, buying crypto");
                    Bot.BuyCrypto();
                }
            }
        }

        public override void ReadFromJson(ref Utf8JsonReader reader)
        {
            base.ReadFromJson(ref reader);
            //reader.GetDecimal()
        }

        private decimal GetRebound(ICryptoExchange api)
        {
            decimal rebound = api.CurrentBuyPrice - lowestPrice;
            decimal reboundPercent = rebound / lowestPrice;
            return reboundPercent;
        }
    }
}
