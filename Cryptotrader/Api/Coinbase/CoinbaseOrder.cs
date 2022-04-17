using System.Text.Json.Serialization;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseOrder : IOrder
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("size")]
        public decimal Amount { get; set; }
        [JsonPropertyName("product_id")]
        public string Product { get; set; }
        [JsonPropertyName("profile_id")]
        public string ProfileID { get; set; }
        [JsonPropertyName("side")]
        public string Side { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("time_in_force")]
        public string TimeInForce { get; set; }
        [JsonPropertyName("post_only")]
        public bool PostOnly { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime Created { get; set; }
        [JsonPropertyName("fill_fees")]
        public decimal FillFees { get; set; }
        [JsonPropertyName("filled_size")]
        public decimal FilledSize { get; set; }
        [JsonPropertyName("executed_value")]
        public decimal ExecutedValue { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("settled")]
        public bool Settled { get; set; }

        public DateTime CreatedAt => Created;
        public OrderType OrderType
        {
            get
            {
                switch (this.Type)
                {
                    case "buy": return OrderType.Buy;
                    case "sell": return OrderType.Sell;
                    default: throw new Exception();
                }
            }
        }
    }
}
