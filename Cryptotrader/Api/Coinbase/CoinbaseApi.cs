using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Cryptotrader.Logging;

namespace Cryptotrader.Api.Coinbase
{
    public class CoinbaseApi : HttpApi
    {
        private readonly string apiKey;
        private readonly string apiSecret;
        private readonly string apiPassphrase;

        private readonly Dictionary<string, CoinbaseAccount> accounts = new();
        private readonly Dictionary<string, CoinbaseWallet> wallets = new();

        public CoinbaseApi(
            Logger logger,
            string url,
            string key,
            string secret,
            string passphrase
        ) : base(logger, url)
        {
            apiKey = key;
            apiSecret = secret;
            apiPassphrase = passphrase;
        }

        public async Task<RequestResult<Dictionary<string, CoinbaseAccount>>> UpdateAccounts()
        {
            return await HandleApiResponse(
                await GetRequest("/accounts"),
                async (res) =>
                {
                    string json = await res.Content.ReadAsStringAsync();
                    var loadedAccounts = Json.Deserialize<CoinbaseAccount[]>(json);
                    foreach (var account in loadedAccounts.Where(a => a.IsTradingEnabled && a.IsInUse))
                    {
                        UpdateAccount(account);
                    }

                    log.Debug($"Loaded {loadedAccounts.Length} valid accounts");
                    return accounts;
                }
            );
        }

        public async Task<RequestResult<Dictionary<string, CoinbaseWallet>>> GetWallets()
        {
            return await HandleApiResponse(
                await GetRequest("/coinbase-accounts"),
                async (res) =>
                {
                    string json = await res.Content.ReadAsStringAsync();
                    var loadedWallets = Json.Deserialize<CoinbaseWallet[]>(json);
                    foreach (var wallet in loadedWallets.Where(w => w.Active))
                    {
                        UpdateWallet(wallet);
                    }

                    log.Debug($"Loaded {loadedWallets.Length} valid wallets");
                    return wallets;
                }
            );
        }

        public async Task<RequestResult<CoinbaseProductTicker>> GetProductTicker(string product)
        {
            return await HandleApiResponse(
                await GetRequest($"/products/{product}/ticker"),
                async (res) =>
                {
                    string json = await res.Content.ReadAsStringAsync();
                    return Json.Deserialize<CoinbaseProductTicker>(json);
                }
            );
        }

        public async Task<RequestResult<CoinbaseOrder>> GetLatestOrder()
        {
            var result = await GetRecentOrders(1);
            return result.CastTo(a => a[0]);
        }
        public async Task<RequestResult<CoinbaseOrder[]>> GetRecentOrders(int limit)
        {
            var request = await GetRequest(
                "/orders",
                new Dictionary<string, string>
                {
                    { "sortedBy", "created_at" },
                    { "sorting", "desc" },
                    { "limit", limit.ToString() },
                    { "status", "done" }
                }
            );
            return await HandleApiResponse(
                request,
                async (res) =>
                {
                    string json = await res.Content.ReadAsStringAsync();
                    return Json.Deserialize<CoinbaseOrder[]>(json);
                }
            );
        }

        public async Task<RequestResult<CoinbaseOrder>> PlaceOrder(decimal amount, string side, string product)
        {
            var request = await PostRequest(
                "/orders",
                new Dictionary<string, object>
                {
                    { "size", amount },
                    { "side", side },
                    { "type", "market" },
                    { "product_id", product }
                }
            );
            return await HandleApiResponse(
                request,
                async (res) =>
                {
                    string json = await res.Content.ReadAsStringAsync();
                    return Json.Deserialize<CoinbaseOrder>(json);
                }
            );
        }

        protected override HttpRequestMessage BuildMessage(string endpoint, HttpMethod method, HttpContent content = null)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string signature = Signature(endpoint, content?.ToString() ?? String.Empty, timestamp, method);

            var request = base.BuildMessage(endpoint, method, content);

            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Cryptotrader", "v0.1"));

            request.Headers.Add("CB-ACCESS-KEY", apiKey);
            request.Headers.Add("CB-ACCESS-SIGN", signature);
            request.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
            request.Headers.Add("CB-ACCESS-PASSPHRASE", apiPassphrase);

            return request;
        }

        private async Task<CoinbaseApiError> ParseErrorMessage(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return Json.Deserialize<CoinbaseApiError>(json);
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

        private void UpdateWallet(CoinbaseWallet wallet)
        {
            string currency = wallet.Currency;
            if (wallets.ContainsKey(currency)) wallets[currency] = wallet;
            else wallets.Add(currency, wallet);
        }

        private async Task<RequestResult<T>> HandleApiResponse<T>(
            HttpResponseMessage response,
            Func<HttpResponseMessage, Task<T>> successPredicate)
        {
            if (response.IsSuccessStatusCode)
            {
                T result = await successPredicate(response);
                return RequestResult<T>.Success(result);
            }
            else
            {
                var error = await ParseErrorMessage(response);
                return RequestResult<T>.Failure(error.Message);
            }
        }
    }
}
