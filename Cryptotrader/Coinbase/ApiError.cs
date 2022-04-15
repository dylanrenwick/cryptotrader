using System.Text.Json.Serialization;

namespace Cryptotrader.Coinbase
{
    internal class ApiError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
