using System.Collections.Generic;

namespace Cryptotrader.Logging
{
    public class MultiLogDestination : LogDestination
    {
        private List<LogDestination> destinations = new();

        public MultiLogDestination(params LogDestination[] destinations)
            : base(LogLevel.Debug)
        {
            this.destinations.AddRange(destinations);
        }

        protected override void LogToTarget(LogMessage message)
        {
            foreach (var destination in destinations)
            {
                destination.Log(message);
            }
        }
    }
}
