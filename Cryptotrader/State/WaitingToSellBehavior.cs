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
            log.Info($"Price is ${api.CurrentSellPrice}, {profit}% higher than last buy price of ${Bot.LastBoughtAt}");
            if (profit > profile.GainThreshold)
            {
                log.Info($"Gain of {profit}% is above gain threshold of {profile.GainThreshold}%");
                log.Alert("Switching to sell rebound");
                Bot.SetState(new SellingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
