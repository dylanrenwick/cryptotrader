using System.Text.Json.Serialization;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseApiError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
