﻿using Cryptotrader.Api.Coinbase;
using Cryptotrader.Config;
using Cryptotrader.Logging;

namespace Cryptotrader
{
    internal class Program
    {
        private const string CONFIG_PATH = @"config.json";

        private static readonly ConfigLoader cfgLoader = new();
        private static Logger log;
        private static Bot bot;

        private static CoinbaseExchange coinbaseDataProvider;

        private static async Task Main(string[] args)
        {
            await LoadConfig();
            log = new Logger(cfgLoader.GetLogDestination());
            log.Debug("Logger initialized");

            try
            {
                log.Debug("Loading API...");
                await CreateDataProvider();
                log.Debug("API loaded");

                log.Debug("Loading Bot...");
                CreateBot();
                log.Debug("Bot loaded");

                log.Debug("Starting Bot...");
                bot.Startup();
            }
            catch (CriticalException)
            {
                throw;
            }
            catch (Exception ex)
            {
                log.Exception(ex);
            }
        }

        private static async Task LoadConfig()
        {
            await cfgLoader.LoadConfig(CONFIG_PATH);
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

            await coinbaseDataProvider.LoadWallets();
        }

        private static void CreateBot()
        {
            bot = new Bot(
                log.Label("BOT"),
                coinbaseDataProvider
            );
        }
    }
}
