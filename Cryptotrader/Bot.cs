using System;
using System.Threading.Tasks;

using Cryptotrader.Config;
using Cryptotrader.Logging;
using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        private DateTime lastStateChange;

        private BotStateBehavior initialState;
        private BotStateBehavior activeState;

        private ICryptoExchange exchange;

        private Logger log;
        private BotConfig config;

        private HistoricalValue<decimal> historicalBuyPrices;
        private HistoricalValue<decimal> historicalSellPrices;

        public Bot(
            Logger logger,
            ICryptoExchange dataProvider,
        ) {
            log = logger;
            exchange = dataProvider;
        }

        public async Task Startup()
        {
            if (config == null) throw new InvalidOperationException("Bot config not loaded!");
            if (initialState == null)
            {
                if (config.InitialState == BotState.None) throw new InvalidOperationException("Initial State not set!");
            }
            SetState(initialState);
            
            await RunBot();
        }

        public void LoadConfig(BotConfig config)
        {
            this.config = config;
        }

        public void BuyCrypto()
        {
            log.Alert($"Buying ${config.AmountToBuy} in crypto");

            exchange.PlaceBuyOrder(config.AmountToBuy);
        }

        public void SellCrypto()
        {
            log.Alert($"Selling ${config.AmountToBuy} in crypto");

            exchange.PlaceSellOrder(config.AmountToBuy);
        }

        private async Task RunBot()
        {
            TimeSpan interval = TimeSpan.FromSeconds(config.UpdateInterval);

            while (activeState != null)
            {
                var rateLimit = Task.Delay(interval);

                Update();

                await rateLimit;
            }
        }

        private void Update()
        {
            exchange.UpdatePrices();

            activeState.Update();
        }

        private void SetState(BotStateBehavior newState)
        {
            activeState?.ExitState();
            activeState = newState;
            activeState.EnterState(this);
            lastStateChange = DateTime.Now;
        }
    }
}
