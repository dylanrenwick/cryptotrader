using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class StartupBehavior : BotStateBehavior
    {
        public override BotState State => Startup;

        private readonly BotState? initialState;

        public StartupBehavior(BotState? initialState)
        {
            this.initialState = initialState;
        }

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            decimal currentPrice = api.CurrentSellPrice;

            Bot.SetLastBuyPrice(currentPrice);
            Bot.SetLastSellPrice(currentPrice);

            BotStateBehavior nextState = CreateInitialState(currentPrice);

            if (nextState == null) nextState = await GetStateByCoinBalance(api, profile);

            await Bot.SetStateAndRun(nextState);
        }

        private static async Task<BotStateBehavior> GetStateByCoinBalance(ICryptoExchange api, BotProfile profile)
        {
            decimal coinBalance = await api.GetWalletBalance(profile.Coin);

            if (coinBalance > 0.02m) return new SellingBehavior(coinBalance);
            else return new BuyingBehavior(coinBalance);
        }

        private BotStateBehavior CreateInitialState(decimal currentPrice)
        {
            return initialState switch
            {
                WaitingToSell => new WaitingToSellBehavior(),
                Selling => new SellingBehavior(currentPrice),
                WaitingToBuy => new WaitingToBuyBehavior(),
                Buying => new BuyingBehavior(currentPrice),
                _ => null,
            };
        }
    }
}
