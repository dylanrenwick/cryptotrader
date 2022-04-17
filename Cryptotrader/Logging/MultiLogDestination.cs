namespace Cryptotrader.Logging
{
    public class MultiLogDestination : ILogDestination
    {
        private List<ILogDestination> destinations = new();

        public MultiLogDestination(params ILogDestination[] destinations)
        {
            this.destinations.AddRange(destinations);
        }

        public void Log(LogMessage message)
        {
            foreach (var destination in destinations)
            {
                destination.Log(message);
            }
        }
    }
}
