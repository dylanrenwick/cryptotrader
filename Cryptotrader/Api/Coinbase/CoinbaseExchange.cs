using Cryptotrader.Logging;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseExchange: ICryptoExchange
    {
        private const string API_URL = "https://api.pro.coinbase.com";

        private readonly string product;

        public decimal CurrentBuyPrice => Math.Round(historicalBuyPrices.Current, 2) + 0.00m;
        public decimal CurrentSellPrice => Math.Round(historicalSellPrices.Current, 2) + 0.00m;

        private readonly HistoricalValue<decimal> historicalBuyPrices = new();
        private readonly HistoricalValue<decimal> historicalSellPrices = new();

        private readonly CoinbaseApi api;
        private readonly Logger log;

        private readonly Dictionary<string, decimal> walletBalances = new();

        public CoinbaseExchange(
            Logger logger,
            string key,
            string secret,
            string passphrase,
            string cash,
            string coin
        ) {
            log = logger;

            if (string.IsNullOrWhiteSpace(key)
                || string.IsNullOrWhiteSpace(secret)
                || string.IsNullOrWhiteSpace(passphrase))
            {
                log.Crit("API info empty or invalid");
            }

            product = $"{coin}-{cash}".ToUpper();

            api = new(log, API_URL, key, secret, passphrase);
        }

        public async Task LoadWallets()
        {
            var result = await api.GetWallets();
            if (result.IsSuccess)
            {
                var wallets = result.Value;
                log.Alert("Api connected and authenticated. Active wallets:");
                foreach ((var currency, var wallet) in wallets.Where(kvp => kvp.Value.Balance > 0))
                {
                    log.Info($"{currency,-6}> {wallet.Balance}");
                    walletBalances.Add(currency, wallet.Balance);
                }
            }
        }

        public async Task<bool> UpdatePrices()
        {
            log.Info("Updating prices");
            Result<CoinbaseProductTicker> response = await api.GetProductTicker(product);
            if (response.IsSuccess)
            {
                CoinbaseProductTicker productTicker = response.Value;
                historicalBuyPrices.Set(productTicker.BuyPrice);
                historicalSellPrices.Set(productTicker.SellPrice);
            }

            return response.IsSuccess;
        }

        public async Task<IOrder> PlaceBuyOrder(decimal amount)
        {
            var response = await api.PlaceOrder(amount, "buy", product);
            return response.Unwrap();
        }
        public async Task<IOrder> PlaceSellOrder(decimal amount)
        {
            var response = await api.PlaceOrder(amount, "sell", product);
            return response.Unwrap();
        }

        public async Task<decimal> GetWalletBalance(string currency, bool forceRefresh)
        {
            if (forceRefresh) await LoadWallets();
            if (walletBalances.ContainsKey(currency)) return walletBalances[currency];
            else return 0m;
        }

        public async Task<IOrder> GetLatestOrder()
        {
            var response = await api.GetLatestOrder();
            return response.Unwrap();
        }
        public async Task<IOrder[]> GetRecentOrders(int limit)
        {
            var response = await api.GetRecentOrders(limit);
            return response.Unwrap();
        }
    }
}
