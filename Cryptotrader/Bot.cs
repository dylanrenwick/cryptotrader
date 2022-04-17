using Cryptotrader.Api;
using Cryptotrader.Config;
using Cryptotrader.Logging;
using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        public decimal LastBoughtAt => historicalBuyPrices.Current;
        public decimal LastSoldAt => historicalSellPrices.Current;

        public bool IsSimulation { get; private set; }

        private DateTime lastStateChange;

        private BotStateBehavior initialState;
        private BotStateBehavior activeState;

        private ICryptoExchange exchange;

        private Logger log;
        private BotConfig config;
        private BotProfile profile => config.Profile;

        private HistoricalValue<decimal> historicalBuyPrices = new();
        private HistoricalValue<decimal> historicalSellPrices = new();

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
                if (profile.InitialState == BotState.None) throw new InvalidOperationException("Initial State not set!");
                initialState = BotStateBehavior.BehaviorFromBotState(profile.InitialState);
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
            log.Alert($"Buying ${profile.LiquidValue} in crypto");
            if (IsSimulation) return;

            exchange.PlaceBuyOrder(profile.LiquidValue);
        }

        public void SellCrypto()
        {
            log.Alert($"Selling ${profile.LiquidValue} in crypto");
            if (IsSimulation) return;

            exchange.PlaceSellOrder(profile.LiquidValue);
        }

        public void SetState(BotStateBehavior newState)
        {
            if (activeState != null) log.Debug($"Exiting {activeState.State}");
            activeState?.ExitState();
            activeState = newState;
            log.Debug($"Entering {activeState.State}");
            activeState.EnterState(this, log);
            lastStateChange = DateTime.Now;
        }

        public void SetLastSellPrice(decimal val)
        {
            historicalSellPrices.Set(val);
        }
        public void SetLastBuyPrice(decimal val)
        {
            historicalBuyPrices.Set(val);
        }

        private async Task RunBot()
        {
            TimeSpan interval = TimeSpan.FromSeconds(config.UpdateInterval);

            while (activeState != null)
            {
                var rateLimit = Task.Delay(interval);

                await Update();

                await rateLimit;
            }
        }

        private async Task Update()
        {
            await exchange.UpdatePrices();

            await activeState.Update(exchange, profile);
        }
    }
}
