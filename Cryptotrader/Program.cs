using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Cryptotrader.Coinbase;
using Cryptotrader.Config;
using Cryptotrader.Logging;

namespace Cryptotrader
{
    internal class Program
    {
        private const string CONFIG_PATH = @"config.json";

        private static readonly ConfigLoader cfgLoader = new();
        private static Logger logger;

        private static CoinbaseDataProvider coinbaseDataProvider;

        private static async Task Main(string[] args)
        {
            await LoadConfig();
            logger = new Logger(cfgLoader.GetLogDestination());
            logger.Debug("Logger initialized");

            logger.Debug("Loading API...");
            await CreateDataProvider();
            logger.Debug("API Loaded");
        }

        private static async Task LoadConfig()
        {
            if (!await cfgLoader.LoadConfig(CONFIG_PATH)) throw new Exception("Could not load config");
        }

        private static async Task CreateDataProvider()
        {
            var apiConfig = cfgLoader.GetApiConfig();
            var apiLogger = logger.Label("API");

            coinbaseDataProvider = new(
                apiLogger,
                apiConfig.ApiKey,
                apiConfig.ApiSecret,
                apiConfig.ApiPassphrase,
                apiConfig.Cash,
                apiConfig.Coin
            );

            await coinbaseDataProvider.FetchAccounts();
        }
    }
}
