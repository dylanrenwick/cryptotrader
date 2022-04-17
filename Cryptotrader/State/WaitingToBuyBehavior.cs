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
            decimal profitShift = Math.Round(profit / Bot.LastSoldAt, 6);
            log.Info($"Price is ${api.CurrentBuyPrice}, {profitShift}% lower than last buy price of ${Bot.LastSoldAt}");
            if (profitShift > profile.LossThreshold)
            {
                log.Info($"Drop of {profitShift}% is above drop threshold of {profile.LossThreshold}%");
                log.Alert("Switching to buy rebound");
                Bot.SetState(new BuyingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
