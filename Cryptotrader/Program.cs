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
        private static Logger log;

        private static CoinbaseDataProvider coinbaseDataProvider;

        private static async Task Main(string[] args)
        {
            await LoadConfig();
            log = new Logger(cfgLoader.GetLogDestination());
            log.Debug("Logger initialized");

            log.Debug("Loading API...");
            await CreateDataProvider();
            log.Debug("API loaded");
        }

        private static async Task LoadConfig()
        {
            if (!await cfgLoader.LoadConfig(CONFIG_PATH)) throw new Exception("Could not load config");
        }

        private static async Task CreateDataProvider()
        {
            var apiConfig = cfgLoader.GetApiConfig();
            var apiLogger = log.Label("API");

            coinbaseDataProvider = new(
                apiLogger,
                apiConfig.ApiKey,
                apiConfig.ApiSecret,
                apiConfig.ApiPassphrase,
                apiConfig.Cash,
                apiConfig.Coin
            );

            if (coinbaseDataProvider.IsSimulation)
            {
                log.Alert("API is in Simulation Mode");
                log.Info("API transactions will not be sent to the API");
                log.Info("Transactions will not be made, only simulated");
            }

            await coinbaseDataProvider.FetchAccounts();
        }
    }
}
