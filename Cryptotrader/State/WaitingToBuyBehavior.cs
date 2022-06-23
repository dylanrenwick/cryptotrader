using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class WaitingToBuyBehavior : BotStateBehavior
    {
        public override BotState State => WaitingToBuy;

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            decimal profit = Bot.GetBuyProfit();
            decimal targetPrice = Math.Round(Bot.LastSoldAt * (1 - profile.LossThreshold / 100), 2);
            log.Debug($"Price is ${api.CurrentBuyPrice}, {Math.Abs(profit)}% {(profit < 0 ? "lower" : "higher")} than last buy price of ${Bot.LastSoldAt}");
            log.Debug($"Configured threshold is {profile.LossThreshold}%, which will be met at a price of ${targetPrice}");
            log.Info($"Price: ${api.CurrentBuyPrice}/${targetPrice} Loss: {profit}%/{profile.LossThreshold}%");
            if (profit > profile.LossThreshold)
            {
                log.Info($"Drop of {profit}% is above drop threshold of {profile.LossThreshold}%");
                log.Alert("Switching to buy rebound");
                Bot.SetState(new BuyingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
