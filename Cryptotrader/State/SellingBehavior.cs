using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class SellingBehavior : BotStateBehavior
    {
        public override BotState State => Selling;

        private decimal highestPrice;

        public SellingBehavior()
        {
            highestPrice = decimal.MinValue;
        }
        public SellingBehavior(decimal high)
        {
            highestPrice = high;
        }

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            if (api.CurrentSellPrice > highestPrice)
            {
                log.Debug($"New highest price of {api.CurrentSellPrice}, previous highest was {highestPrice}");
                highestPrice = api.CurrentSellPrice;
            }
            else if (api.CurrentSellPrice < highestPrice)
            {
                log.Debug($"Price of {api.CurrentSellPrice} is lower than high of {highestPrice}, checking rebound threshold");
                decimal reboundAmount = GetRebound(api);
                log.Debug($"Price has rebounded by {reboundAmount * 100}% Threshold is {profile.RiseEndThreshold}%");
                if (reboundAmount >= profile.FallEndThreshold)
                {
                    log.Info($"Rebound amount of {reboundAmount * 100}% is greater than threshold of {profile.RiseEndThreshold}%");
                    log.Info("Price has stopped rising, selling crypto");
                    await Bot.SellCrypto();
                }
            }
        }

        private decimal GetRebound(ICryptoExchange api)
        {
            decimal rebound = highestPrice - api.CurrentSellPrice;
            decimal reboundPercent = rebound / highestPrice;
            return reboundPercent;
        }
    }
}
