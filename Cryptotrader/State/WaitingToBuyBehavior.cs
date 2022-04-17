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
            if (profit > Bot.LastSoldAt * profile.LossThreshold)
            {
                log.Info($"Price is ${api.CurrentBuyPrice}, {profit / Bot.LastSoldAt}% lower than last sell price of ${Bot.LastSoldAt}");
                log.Info($"Drop of {profit / Bot.LastSoldAt}% is above drop threshold of {profile.LossThreshold}%");
                log.Info("Switching to buy rebound");
                Bot.SetState(new BuyingBehavior(api.CurrentBuyPrice));
            }
        }
    }
}
