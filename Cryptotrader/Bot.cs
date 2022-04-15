using System;
using System.Threading.Tasks;

using Cryptotrader.Config;
using Cryptotrader.Logging;
using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        public decimal DropBeforeBuy => config.DropBeforeBuy;
        public decimal ReboundBeforeBuy => config.ReboundBeforeBuy;
        public decimal GainBeforeSell => config.GainBeforeSell;
        public decimal ReboundBeforeSell => config.ReboundBeforeSell;

        public decimal LastBoughtAt => historicalBuyPrices.Current;
        public decimal LastSoldAt => historicalSellPrices.Current;

        public decimal CurrentBuyPrice => exchange.CurrentBuyPrice;
        public decimal CurrentSellPrice => exchange.CurrentSellPrice;

        public bool IsSimulation { get; private set; }

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
            bool sim = false
        ) {
            log = logger;
            exchange = dataProvider;
            IsSimulation = sim;
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

        public decimal GetBuyProfit()
        {
            decimal buyPrice = exchange.CurrentBuyPrice;
            decimal profit = historicalSellPrices.Current - buyPrice;
            return profit;
        }
        public decimal GetSellProfit()
        {
            decimal sellPrice = exchange.CurrentSellPrice;
            decimal profit = sellPrice - historicalBuyPrices.Current;
            return profit;
        }

        public void BuyCrypto()
        {
            log.Alert($"Buying ${config.AmountToBuy} in crypto");
            if (IsSimulation) return;

            exchange.PlaceBuyOrder(config.AmountToBuy);
        }

        public void SellCrypto()
        {
            log.Alert($"Selling ${config.AmountToBuy} in crypto");
            if (IsSimulation) return;

            exchange.PlaceSellOrder(config.AmountToBuy);
        }

        public void SetState(BotStateBehavior newState)
        {
            activeState?.ExitState();
            activeState = newState;
            activeState.EnterState(this, log);
            lastStateChange = DateTime.Now;
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
    }
}
