namespace Cryptotrader.Api
{
    public interface IOrder
    {
        public DateTime CreatedAt { get; }
        public decimal Amount { get; }
        public decimal Value { get; }
        public OrderType OrderType { get; }
        public string Currency { get; }
    }

    public enum OrderType
    {
        Buy,
        Sell
    }
}
