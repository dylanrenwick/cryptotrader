using Cryptotrader.Logging;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseExchange: ICryptoExchange
    {
        private const string API_URL = "https://api.pro.coinbase.com";

        private string cash;
        private string coin;

        private string product => $"{coin}-{cash}";

        public decimal CurrentBuyPrice => historicalBuyPrices.Current;
        public decimal CurrentSellPrice => historicalSellPrices.Current;
        private HistoricalValue<decimal> historicalBuyPrices;
        private HistoricalValue<decimal> historicalSellPrices;

        private readonly CoinbaseApi api;
        private readonly Logger log;

        public CoinbaseExchange(
            Logger logger,
            string key,
            string secret,
            string passphrase,
            string cash,
            string coin
        ) {
            log = logger;

            historicalBuyPrices = new HistoricalValue<decimal>();
            historicalSellPrices = new HistoricalValue<decimal>();

            if (string.IsNullOrWhiteSpace(key)
                || string.IsNullOrWhiteSpace(secret)
                || string.IsNullOrWhiteSpace(passphrase))
            {
                log.Crit("API info empty or invalid");
            }

            this.cash = cash.ToUpper();
            this.coin = coin.ToUpper();

            api = new(log, API_URL, key, secret, passphrase);
        }

        public async Task LoadWallets()
        {
            var result = await api.GetWallets();
            if (result.IsSuccess)
            {
                var wallets = result.Result;
                foreach ((var currency, var wallet) in wallets)
                {
                    if (wallet.Balance > 0) log.Info($"{currency.PadRight(6)}> {wallet.Balance}");
                }
            }
        }

        public async Task UpdatePrices()
        {
            log.Info("Updating prices");
            RequestResult<CoinbaseProductTicker> response = await api.GetProductTicker(product);
            if (response.IsSuccess)
            {
                CoinbaseProductTicker productTicker = response.Result;
                historicalBuyPrices.Set(productTicker.BuyPrice);
                historicalSellPrices.Set(productTicker.SellPrice);
            }
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

        public async Task<IOrder> GetLatestOrder()
        {
            var response = await api.GetLatestOrder();
            return response.Unwrap();
        }
    }
}
