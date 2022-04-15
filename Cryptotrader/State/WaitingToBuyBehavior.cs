using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class WaitingToBuyBehavior : BotStateBehavior
    {
        public override BotState State => WaitingToBuy;

        public override void Update()
        {
            decimal profit = Bot.GetBuyProfit();
            if (profit > Bot.LastSoldAt * Bot.DropBeforeBuy)
            {
                log.Info($"Price is ${Bot.CurrentBuyPrice}, {profit / Bot.LastSoldAt}% lower than last sell price of ${Bot.LastSoldAt}");
                log.Info($"Drop of {profit / Bot.LastSoldAt}% is above drop threshold of {Bot.DropBeforeBuy}%");
                log.Info("Switching to buy rebound");
                Bot.SetState(new BuyingBehavior(Bot.CurrentBuyPrice));
            }
        }
    }
}
