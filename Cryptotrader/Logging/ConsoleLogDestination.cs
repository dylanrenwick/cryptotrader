using System;

namespace Cryptotrader.Logging
{
    public class ConsoleLogDestination : LogDestination
    {
        public ConsoleLogDestination(LogLevel maxLevel)
            : base(maxLevel) { }

        protected override void LogToTarget(LogMessage message)
        {
            Console.Write(message);
        }
    }
}
