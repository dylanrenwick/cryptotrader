namespace Cryptotrader.Logging
{
    public class ConsoleLogDestination : LeveledLogDestination
    {
        public ConsoleLogDestination(LogLevel maxLevel)
            : base(maxLevel) { }

        protected override void LogToTarget(LogMessage message)
        {
            Console.Write(message);
        }
    }
}
