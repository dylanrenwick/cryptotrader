namespace Cryptotrader
{
    public class HistoricalValue<T>
    {
        public T Current { get; private set; }

        private Dictionary<DateTime, T> history;

        public HistoricalValue()
        {
            history = new Dictionary<DateTime, T>();
        }

        public void Set(T newValue)
        {
            history.Add(DateTime.Now, newValue);
            Current = newValue;
        }
    }
}
