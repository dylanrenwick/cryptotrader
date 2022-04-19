using System.Net;
using System.Net.Http;
using System.Web;

using Cryptotrader.Logging;

namespace Cryptotrader.Api
{
    public class HttpApi
    {
        protected readonly string apiUrl;
        protected readonly Logger log;

        public int RetryLimit { get; set; }
        public int RetryDelay { get; set; }

        public HttpApi(Logger logger, string url)
        {
            log = logger;
            apiUrl = url;

            RetryLimit = 3;
            RetryDelay = 5000;
        }

        public async Task<HttpResponseMessage> GetRequest(string endpoint, Dictionary<string, string> queryData)
        {
            IEnumerable<string> queryPairs = queryData.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}");
            if (queryPairs.Any())
            {
                string queryString = string.Join('&', queryPairs);
                return await GetRequest($"{endpoint}?{queryString}");
            }
            else
            {
                return await GetRequest(endpoint);
            }
        }
        public async Task<HttpResponseMessage> GetRequest(string endpoint)
        {
            log.Debug($"Requesting {endpoint}");

            var request = BuildMessage(endpoint, HttpMethod.Get);
            return await SendRequest(request);
        }

        public async Task<HttpResponseMessage> PostRequest(string endpoint, Dictionary<string, object> postData)
            => await PostRequest(endpoint, Json.Serialize(postData));
        public async Task<HttpResponseMessage> PostRequest(string endpoint, string postData)
        {
            log.Debug($"Requesting {endpoint} with\n{postData}");

            var request = BuildMessage(endpoint, HttpMethod.Post, new StringContent(postData));
            return await SendRequest(request);
        }

        public async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
        {
            return await TryRequest(request);
        }

        protected HttpRequestMessage BuildMessage(string endpoint, HttpMethod method, string content)
            => BuildMessage(endpoint, method, new StringContent(content));
        protected virtual HttpRequestMessage BuildMessage(string endpoint, HttpMethod method, HttpContent content = null)
        {
            HttpRequestMessage request = new(method, endpoint);

            request.Content = content;

            return request;
        }

        private async Task<HttpResponseMessage> TryRequest(HttpRequestMessage request, int retries = 0)
        {
            HttpResponseMessage response = null;

            using HttpClient web = new();

            web.BaseAddress = new Uri(apiUrl);

            try
            {
                response = await web.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                if (IsRetryable(ex.StatusCode))
                {
                    if (RetryLimit > 0 && retries >= RetryLimit)
                        throw new HttpRequestException(
                            $"Failed to get response after {retries} retries",
                            ex.InnerException,
                            ex.StatusCode);

                    await Task.Delay(RetryDelay);
                    return await TryRequest(request, retries++);
                }
            }

            return response;
        }

        private static readonly HttpStatusCode[] retryableCodes = new[]
        {
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.GatewayTimeout
        };

        private static bool IsRetryable(HttpStatusCode? statusCode)
        {
            return statusCode != null && retryableCodes.Contains(statusCode.Value);
        }
    }
}
