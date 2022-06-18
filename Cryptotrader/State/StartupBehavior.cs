using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class StartupBehavior : BotStateBehavior
    {
        public override BotState State => Startup;

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            IOrder order = await api.GetLatestOrder();
            decimal tradeValue = order.Value / order.Amount;
            string[] productParts = order.Currency.Split('-');
            string coin = productParts[0];
            string currency = productParts[1];
            log.Info($"Detected last {order.OrderType} of {order.Amount} {coin} for ${Math.Round(order.Value,2)} at ${Math.Round(tradeValue,2)} each");

            BotStateBehavior nextState = null;
            switch (order.OrderType)
            {
                case OrderType.Buy:
                    Bot.SetLastBuyPrice(tradeValue);
                    nextState = new WaitingToSellBehavior();
                    break;
                case OrderType.Sell:
                    Bot.SetLastSellPrice(tradeValue);
                    nextState = new WaitingToBuyBehavior();
                    break;
            }

            await Bot.SetStateAndRun(nextState);
        }
    }
}
