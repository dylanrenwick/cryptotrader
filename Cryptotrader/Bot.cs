using Cryptotrader.Api;
using Cryptotrader.Config;
using Cryptotrader.Logging;
using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        public decimal LastBoughtAt => Math.Round(historicalBuyPrices.Current, 2);
        public decimal LastSoldAt => Math.Round(historicalSellPrices.Current, 2);

        public bool IsSimulation { get; private set; }

        private DateTime startTime;
        private DateTime lastStateChange;

        private BotStateBehavior initialState;
        private BotStateBehavior activeState;

        private readonly ICryptoExchange exchange;

        private readonly Logger log;
        private BotConfig config;
        private BotProfile profile;

        private readonly HistoricalValue<decimal> historicalBuyPrices = new();
        private readonly HistoricalValue<decimal> historicalSellPrices = new();

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

            log.Alert("Cryptotrader starting up");

            if (IsSimulation)
            {
                log.Alert("Starting up in simulation mode!");
                log.Info("Buy/Sell orders will not be sent to the API, but the bot will otherwise function normally");
            }
            
            await RunBot();
        }

        public void LoadConfig(BotConfig config)
        {
            this.config = config;
            this.profile = config.Profile;
        }

        public decimal GetBuyProfit()
        {
            decimal buyPrice = exchange.CurrentBuyPrice;
            decimal profit = LastSoldAt - buyPrice;
            decimal profitPercent = profit / LastSoldAt * 100;
            return Math.Round(profitPercent, 4);
        }
        public decimal GetSellProfit()
        {
            decimal sellPrice = exchange.CurrentSellPrice;
            decimal profit = sellPrice - LastBoughtAt;
            decimal profitPercent = profit / LastBoughtAt * 100;
            return Math.Round(profitPercent, 4);
        }

        public async Task BuyCrypto()
        {
            log.Alert($"Buying ${profile.LiquidValue} in crypto");
            historicalBuyPrices.Set(exchange.CurrentBuyPrice);
            if (IsSimulation) return;

            await exchange.PlaceBuyOrder(profile.LiquidValue);
        }

        public async Task SellCrypto()
        {
            log.Alert($"Selling ${profile.LiquidValue} in crypto");
            historicalSellPrices.Set(exchange.CurrentSellPrice);
            if (IsSimulation) return;

            await exchange.PlaceSellOrder(profile.LiquidValue);
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

        public async Task SetStateAndRun(BotStateBehavior newState)
        {
            SetState(newState);
            await UpdateState();
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
            startTime = DateTime.Now;

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
            DebugLog();

            if (activeState.State == BotState.Reconnecting || await exchange.UpdatePrices())
            {
                await UpdateState();
            }
            else
            {
                log.Warn($"Could not connect to exchange API");
                ReconnectingBehavior newState = new(activeState);
                SetState(newState);
            }
        }

        private async Task UpdateState()
        {
            await activeState.Update(exchange, profile);
        }

        private void DebugLog()
        {
            TimeSpan runtime = DateTime.Now.Subtract(startTime);
            TimeSpan stateTime = DateTime.Now.Subtract(lastStateChange);

            log.Debug($"Cryptotrader bot running {runtime:g}{(IsSimulation?" [Simulation]":"")}");
            log.Debug($"Current state is {activeState?.State} ({stateTime:g})");
        }
    }
}
