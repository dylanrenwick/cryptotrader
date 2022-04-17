using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class WaitingToSellBehavior : BotStateBehavior
    {
        public override BotState State => WaitingToSell;

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            decimal profit = Bot.GetSellProfit();
            decimal profitShift = Math.Round(profit / Bot.LastBoughtAt, 6);
            log.Info($"Price is ${api.CurrentSellPrice}, {profitShift}% higher than last buy price of ${Bot.LastBoughtAt}");
            if (profitShift > profile.GainThreshold)
            {
                log.Info($"Gain of {profitShift}% is above gain threshold of {profile.GainThreshold}%");
                log.Info("Switching tosell rebound");
                Bot.SetState(new SellingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
