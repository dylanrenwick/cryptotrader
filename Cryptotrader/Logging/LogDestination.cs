namespace Cryptotrader.Logging
{
    public abstract class LogDestination
    {
        public LogLevel MaxLogLevel { get; }

        public LogDestination(LogLevel maxLevel)
        {
            MaxLogLevel = maxLevel;
        }

        public void Log(LogMessage message)
        {
            if (message.Level <= MaxLogLevel) LogToTarget(message);
        }

        protected abstract void LogToTarget(LogMessage message);
    }
}
