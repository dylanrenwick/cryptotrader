namespace Cryptotrader.Logging
{
    public abstract class LeveledLogDestination : ILogDestination
    {
        public LogLevel MaxLogLevel { get; }

        public LeveledLogDestination(LogLevel maxLevel)
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
