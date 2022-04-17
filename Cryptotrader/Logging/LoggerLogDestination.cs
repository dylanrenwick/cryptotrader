using System;

namespace Cryptotrader.Logging
{
    public class LoggerLogDestination : ILogDestination
    {
        private readonly Logger _logger;

        public LoggerLogDestination(Logger logger)
        {
            _logger = logger;
        }

        public  void Log(LogMessage message)
        {
            if (_logger == null) throw new InvalidOperationException();
            _logger.LogMessage(message);
        }
    }
}
