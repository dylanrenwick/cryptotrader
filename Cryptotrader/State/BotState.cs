namespace Cryptotrader.State
{
    public enum BotState
    {
        None = 0,
        Startup,
        WaitingToSell,
        Selling,
        WaitingToBuy,
        Buying,
        Reconnecting
    }
}
