using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Cryptotrader.Logging;

namespace Cryptotrader.Coinbase
{
    public class CoinbaseExchange : ICryptoExchange
    {
        private const string API_URL = "https://api.pro.coinbase.com";

        public bool IsSimulation { get; set; }

        private string apiKey;
        private string apiSecret;
        private string apiPassphrase;

        private string cash;
        private string coin;

        private string product => $"{coin}-{cash}";

        public decimal CurrentBuyPrice => historicalBuyPrices.Current;
        public decimal CurrentSellPrice => historicalSellPrices.Current;

        private Logger log;
        private HistoricalValue<decimal> historicalBuyPrices;
        private HistoricalValue<decimal> historicalSellPrices;

        private Dictionary<string, CoinbaseAccount> accounts = new Dictionary<string, CoinbaseAccount>();

        public CoinbaseExchange(
            Logger logger,
            string key,
            string secret,
            string passphrase,
            string cash,
            string coin
        ){
            log = logger;
            historicalBuyPrices = new HistoricalValue<decimal>();
            historicalSellPrices = new HistoricalValue<decimal>();

            apiKey = key;
            apiSecret = secret;
            apiPassphrase = passphrase;

            if (string.IsNullOrWhiteSpace(apiKey)
                || string.IsNullOrWhiteSpace(apiSecret)
                || string.IsNullOrWhiteSpace(apiPassphrase))
            {
                log.Crit("API info empty or invalid, defaulting to Simulation Mode");
            }

            this.cash = cash.ToUpper();
            this.coin = coin.ToUpper();
        }

        public async Task UpdatePrices()
        {
            log.Info("Updating prices");
            RequestResult<ProductTicker> response = await GetProductTicker();
            if (response.IsSuccess)
            {
                ProductTicker productTicker = response.Result;
                historicalBuyPrices.Set(productTicker.BuyPrice);
                historicalSellPrices.Set(productTicker.SellPrice);
            }
        }

        public async Task<bool> PlaceBuyOrder(decimal amount)
        {
            var response = await PostRequest(
                "/orders",
                new Dictionary<string, object>
                {
                    { "size", amount },
                    { "side", "buy" },
                    { "type", "market" },
                    { "product_id", product }
                }
            );

            return response.IsSuccess;
        }
        public async Task<bool> PlaceSellOrder(decimal amount)
        {
            var response = await PostRequest(
                "/orders",
                new Dictionary<string, object>
                {
                    { "size", amount },
                    { "side", "sell" },
                    { "type", "market" },
                    { "product_id", product }
                }
            );

            return response.IsSuccess;
        }

        public async Task FetchAccounts()
        {
            RequestResult<string> response = await GetRequest("/accounts");
            if (response.IsSuccess)
            {
                string json = response.Result;
                CoinbaseAccount[] accounts = JsonSerializer.Deserialize<CoinbaseAccount[]>(json);

                foreach (var account in accounts.Where(a => a.IsTradingEnabled && a.IsInUse))
                {
                    UpdateAccount(account);
                }

                log.Debug($"Loaded {accounts.Count()} valid accounts");
            }
        }

        private async Task<RequestResult<ProductTicker>> GetProductTicker()
        {
            RequestResult<string> response = await GetRequest($"/products/{product}/ticker");
            if (response.IsSuccess)
            {
                string json = response.Result;
                var productTicker = JsonSerializer.Deserialize<ProductTicker>(json);
                return RequestResult<ProductTicker>.Success(productTicker);
            }
            else
            {
                return RequestResult<ProductTicker>.Failure(response.ErrorMessage);
            }
        }

        private async Task<RequestResult<string>> PostRequest(string endpoint, Dictionary<string, object> postData)
            => await PostRequest(endpoint, JsonSerializer.Serialize(postData));
        private async Task<RequestResult<string>> PostRequest(string endpoint, string postData)
        {
            if (IsSimulation) return RequestResult<string>.Success(string.Empty); ;

            log.Debug($"Requesting {endpoint} with\n{postData}");

            var request = BuildMessage(endpoint, HttpMethod.Post);
            request.Content = new StringContent(postData);
            return await SendRequest(request);
        }

        private async Task<RequestResult<string>> GetRequest(string endpoint)
        {
            if (IsSimulation) return RequestResult<string>.Success(string.Empty);

            log.Debug($"Requesting {endpoint}");

            var request = BuildMessage(endpoint, HttpMethod.Get);
            return await SendRequest(request);
        }

        private async Task<RequestResult<string>> SendRequest(HttpRequestMessage request)
        {
            using (HttpClient web = new())
            {
                web.BaseAddress = new Uri(API_URL);
                HttpResponseMessage response = await web.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return RequestResult<string>.Success(responseContent);
                }
                else
                {
                    ApiError error = await ParseErrorMessage(response);
                    return RequestResult<string>.Failure(error.Message);
                }
            }
        }

        private async Task<ApiError> ParseErrorMessage(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<ApiError>(stream);
        }

        private HttpRequestMessage BuildMessage(string endpoint, HttpMethod method)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            HttpRequestMessage request = new(method, endpoint);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Cryptotrader", "v0.1"));
            request.Headers.Add("CB-ACCESS-KEY", apiKey);
            request.Headers.Add("CB-ACCESS-SIGN", Signature(endpoint, "", timestamp, method));
            request.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
            request.Headers.Add("CB-ACCESS-PASSPHRASE", apiPassphrase);
            return request;
        }

        private string Signature(string endpoint, string body, string timestamp, HttpMethod method)
        {
            StringBuilder sb = new();
            sb.Append(timestamp);
            sb.Append(method.ToString());
            sb.Append(endpoint);
            sb.Append(body);

            byte[] signatureContent = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] secretBytes = Convert.FromBase64String(apiSecret);

            using HMACSHA256 hmac = new(secretBytes);
            byte[] hashData = hmac.ComputeHash(signatureContent);
            string b64 = Convert.ToBase64String(hashData);
            return b64;
        }

        private void UpdateAccount(CoinbaseAccount account)
        {
            string currency = account.Currency;
            if (accounts.ContainsKey(currency)) accounts[currency] = account;
            else accounts.Add(currency, account);
        }
    }
}
