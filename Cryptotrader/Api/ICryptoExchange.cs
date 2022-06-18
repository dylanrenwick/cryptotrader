namespace Cryptotrader.Api
{
    public interface ICryptoExchange
    {
        public decimal CurrentBuyPrice { get; }
        public decimal CurrentSellPrice { get; }

        public Task<bool> UpdatePrices();

        public Task<IOrder> PlaceBuyOrder(decimal amount);
        public Task<IOrder> PlaceSellOrder(decimal amount);

        public Task<IOrder> GetLatestOrder();
        public Task<IOrder[]> GetRecentOrders(int limit);
    }
}
