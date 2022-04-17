using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Cryptotrader.Logging;

namespace Cryptotrader.Config
{
    public class ConfigLoader
    {
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

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

        public async Task<bool> LoadConfig(string filePath)
        {
            try
            {
                LoadFile(filePath);
                await ParseConfig();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoadFile(string filePath)
        {
            jsonFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        private async Task ParseConfig()
        {
            configState = await JsonSerializer.DeserializeAsync<ConfigState>(jsonFileStream, jsonOptions);
        }
    }
}
