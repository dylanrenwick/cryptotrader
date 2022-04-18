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
            log.Info($"Price is ${api.CurrentBuyPrice}, {profit}% lower than last buy price of ${Bot.LastSoldAt}");
            if (profit > profile.LossThreshold)
            {
                log.Info($"Drop of {profit}% is above drop threshold of {profile.LossThreshold}%");
                log.Alert("Switching to buy rebound");
                Bot.SetState(new BuyingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
