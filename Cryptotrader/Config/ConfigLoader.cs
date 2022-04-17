using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Cryptotrader.Logging;

namespace Cryptotrader.Config
{
    public class ConfigLoader
    {
        private FileStream jsonFileStream;
        private ConfigState configState;

        public ILogDestination GetLogDestination()
        {
            ILogDestination[] destinations = configState.DestinationConfigs
                .Select(dest => dest.GetDestination()).ToArray();

            if (destinations.Length > 1) return new MultiLogDestination(destinations);
            else if (destinations.Length == 1) return destinations[0];
            else return new ConsoleLogDestination(LogLevel.Debug);
        }

        public ApiConfig GetApiConfig()
        {
            return configState.ApiConfig;
        }
        public BotConfig GetBotConfig()
        {
            return configState.BotConfig;
        }

        public async Task LoadConfig(string filePath)
        {
            LoadFile(filePath);
            await ParseConfig();
        }

        private void LoadFile(string filePath)
        {
            jsonFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        private async Task ParseConfig()
        {
            configState = await Json.DeserializeAsync<ConfigState>(jsonFileStream);
        }
    }
}
