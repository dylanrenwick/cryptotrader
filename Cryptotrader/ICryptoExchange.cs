using System.Threading.Tasks;

namespace Cryptotrader
{
    public interface ICryptoExchange
    {
        public decimal CurrentBuyPrice { get; }
        public decimal CurrentSellPrice { get; }

        public Task UpdatePrices();

        public Task<bool> PlaceBuyOrder(decimal amount);
        public Task<bool> PlaceSellOrder(decimal amount);

    }
}
