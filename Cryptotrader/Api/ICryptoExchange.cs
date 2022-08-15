namespace Cryptotrader.Api
{
    public interface ICryptoExchange
    {
        public decimal CurrentBuyPrice { get; }
        public decimal CurrentSellPrice { get; }

        public Task<bool> UpdatePrices(string product);

        public Task<IOrder> PlaceBuyOrder(string product, decimal amount);
        public Task<IOrder> PlaceSellOrder(string product, decimal amount);

        public Task<IOrder> GetLatestOrder();
        public Task<IOrder[]> GetRecentOrders(int limit);

        public Task<decimal> GetWalletBalance(string currency, bool forceRefresh = false);
    }
}
