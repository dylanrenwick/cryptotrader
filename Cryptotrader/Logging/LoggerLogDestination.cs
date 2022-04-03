using System;

namespace Cryptotrader.Logging
{
    public class LoggerLogDestination : LogDestination
    {
        private readonly Logger _logger;

        public LoggerLogDestination(Logger logger, LogLevel maxLevel)
            : base(maxLevel)
        {
            _logger = logger;
        }

        protected override void LogToTarget(LogMessage message)
        {
            if (_logger == null) throw new InvalidOperationException();
            _logger.LogMessage(message);
        }
    }
}
