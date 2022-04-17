namespace Cryptotrader.Api
{
    public interface IOrder
    {
        public DateTime CreatedAt { get; }
        public decimal Amount { get; }
        public OrderType OrderType { get; }
    }

    public enum OrderType
    {
        Buy,
        Sell
    }
}
