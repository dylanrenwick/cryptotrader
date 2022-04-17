using System;
using System.Text.Json.Serialization;

namespace Cryptotrader.Api.Coinbase
{
    internal class CoinbaseProductTicker
    {
        [JsonPropertyName("trade_id")]
        public int TradeID { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("size")]
        public decimal Size { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
        [JsonPropertyName("bid")]
        public decimal BuyPrice { get; set; }
        [JsonPropertyName("ask")]
        public decimal SellPrice { get; set; }
        [JsonPropertyName("volume")]
        public decimal DayVolume { get; set; }
    }
}
