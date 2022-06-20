using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class StartupBehavior : BotStateBehavior
    {
        public override BotState State => Startup;

        private decimal lastOrderValue;
        private OrderType lastOrderType;

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            if (profile.LastBuyPrice.HasValue && profile.LastSellPrice.HasValue)
            {
                log.Crit($"Cannot determine initial state");
            }
            else if (profile.LastBuyPrice.HasValue)
            {
                lastOrderValue = profile.LastBuyPrice.Value;
                lastOrderType = OrderType.Buy;
            }
            else if (profile.LastSellPrice.HasValue)
            {
                lastOrderValue = profile.LastSellPrice.Value;
                lastOrderType = OrderType.Sell;
            }
            else
            {
                await FetchLatestOrder(api);
            }

            BotStateBehavior nextState = null;
            switch (lastOrderType)
            {
                case OrderType.Buy:
                    Bot.SetLastBuyPrice(lastOrderValue);
                    nextState = new WaitingToSellBehavior();
                    break;
                case OrderType.Sell:
                    Bot.SetLastSellPrice(lastOrderValue);
                    nextState = new WaitingToBuyBehavior();
                    break;
            }

            await Bot.SetStateAndRun(nextState);
        }

        private async Task FetchLatestOrder(ICryptoExchange api)
        {
            IOrder order = await api.GetLatestOrder();
            string coin = order.Currency.Split('-')[0];
            lastOrderValue = order.Value / order.Amount;
            log.Info($"Detected last {order.OrderType} of {order.Amount} {coin} for ${Math.Round(order.Value, 2)} at ${Math.Round(lastOrderValue, 2)} each");
            lastOrderType = order.OrderType;
        }
    }
}
