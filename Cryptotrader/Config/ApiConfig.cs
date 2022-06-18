using System.Text.Json.Serialization;

namespace Cryptotrader.Config
{
    public class ApiConfig
    {
        [JsonPropertyName("key")]
        public string ApiKey { get; set; }
        [JsonPropertyName("secret")]
        public string ApiSecret { get; set; }
        [JsonPropertyName("passphrase")]
        public string ApiPassphrase { get; set; }

        [JsonPropertyName("cash")]
        public string Cash { get; set; }
        [JsonPropertyName("coin")]
        public string Coin { get; set; }
    }
}
