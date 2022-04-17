using System.Text.Json.Serialization;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseWallet
    {
        [JsonPropertyName("available_on_consumer")]
        public bool AvailableOnConsumer { get; set; }
        [JsonPropertyName("hold_balance")]
        public decimal HoldBalance { get; set; }
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("hold_currency")]
        public string HoldCurrency { get; set; }
        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("primary")]
        public bool IsPrimary { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }
}
