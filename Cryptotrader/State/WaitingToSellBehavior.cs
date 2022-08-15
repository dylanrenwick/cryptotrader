using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class WaitingToSellBehavior : BotStateBehavior
    {
        public override BotState State => WaitingToSell;

#pragma warning disable CS1998
        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            decimal profit = Bot.GetSellProfit();
            decimal targetPrice = Math.Round(Bot.LastBoughtAt * (1 + profile.GainThreshold / 100), 2);
            log.Debug($"Price is ${api.CurrentSellPrice}, {Math.Abs(profit)}% {(profit > 0 ? "higher" : "lower")} than last buy price of ${Bot.LastBoughtAt}");
            log.Debug($"Configured threshold is {profile.GainThreshold}%, which will be met at a price of ${targetPrice}");
            log.Info($"Price: ${api.CurrentSellPrice}/${targetPrice} Loss: {profit}%/{profile.GainThreshold}%");
            if (profit > profile.GainThreshold)
            {
                log.Info($"Gain of {profit}% is above gain threshold of {profile.GainThreshold}%");
                log.Alert("Switching to sell rebound");
                Bot.SetState(new SellingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
