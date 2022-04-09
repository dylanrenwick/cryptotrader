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
    public class CoinbaseDataProvider
    {
        private const string API_URL = "https://api.pro.coinbase.com";

        public bool IsSimulation { get; set; }

        private string apiKey;
        private string apiSecret;
        private string apiPassphrase;

        private string cash;
        private string coin;

        private string product => $"{cash}-{coin}";

        private Logger log;

        private Dictionary<string, CoinbaseAccount> accounts = new Dictionary<string, CoinbaseAccount>();

        public CoinbaseDataProvider(
            Logger logger,
            string key,
            string secret,
            string passphrase,
            string cash,
            string coin
        ){
            log = logger;

            apiKey = key;
            apiSecret = secret;
            apiPassphrase = passphrase;

            if (string.IsNullOrWhiteSpace(apiKey)
                || string.IsNullOrWhiteSpace(apiSecret)
                || string.IsNullOrWhiteSpace(apiPassphrase))
            {
                log.Info("API info empty or invalid, defaulting to Simulation Mode");
                IsSimulation = true;
            }

            this.cash = cash.ToUpper();
            this.coin = coin.ToUpper();
        }

        public decimal GetCryptoSellPrice()
        {
            throw new NotImplementedException();
        }
        public decimal GetCryptoBuyPrice()
        {
            throw new NotImplementedException();
        }

        public void PlaceSellOrder()
        {
            throw new NotImplementedException();
        }
        public void PlaceBuyOrder()
        {
            throw new NotImplementedException();
        }

        public async Task FetchAccounts()
        {
            string response = await GetRequest("/accounts");
            CoinbaseAccount[] accounts = JsonSerializer.Deserialize<CoinbaseAccount[]>(response);
            accounts.Where(a => a.IsTradingEnabled && a.IsInUse)
                .ToList().ForEach(a => UpdateAccount(a));
            log.Debug($"Loaded {accounts.Count()} valid accounts");
        }

        private async Task PostRequest(string endpoint, string postData)
        {
            if (IsSimulation) return;

            log.Debug($"Requesting {endpoint} with\n{postData}");
            using (var web = new HttpClient())
            {
                web.BaseAddress = new Uri(API_URL);
                var request = BuildMessage(endpoint, HttpMethod.Post);
                request.Content = new StringContent(postData);
                var response = await web.SendAsync(request);
                log.Debug($"Response is {response.Content.ToString()}");
            }
        }

        private async Task<string> GetRequest(string endpoint)
        {
            if (IsSimulation) return string.Empty;

            log.Debug($"Requesting {endpoint}");
            using (var web = new HttpClient())
            {
                web.BaseAddress = new Uri(API_URL);
                var request = BuildMessage(endpoint, HttpMethod.Get);
                var response = await web.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                log.Debug($"Response is {responseString}");
                return responseString;
            }
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
