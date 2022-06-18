using Cryptotrader.Api.Coinbase;
using Cryptotrader.Config;
using Cryptotrader.Logging;

namespace Cryptotrader
{
    internal class Program
    {
        private static string configPath = @"config.json";

        private static readonly ConfigLoader cfgLoader = new();
        private static Logger log;
        private static Bot bot;

        private static CoinbaseExchange coinbaseDataProvider;

        private static bool isSimulation;

        private static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                LoadArgs(args);
            }

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
                await bot.Startup();
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

        private static void LoadArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-sim") isSimulation = true;
                else if (args[i] == "-c")
                {
                    if (i == args.Length - 1) throw new Exception("Expected value for -c flag");
                    configPath = args[++i];
                }
            }
        }

        private static async Task LoadConfig()
        {
            await cfgLoader.LoadConfig(configPath);
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
                coinbaseDataProvider,
                isSimulation
            );

            bot.LoadConfig(cfgLoader.GetBotConfig());
        }
    }
}
