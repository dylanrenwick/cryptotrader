using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class WaitingToSellBehavior : BotStateBehavior
    {
        public override BotState State => WaitingToSell;

        public override void Update(ICryptoExchange api, BotProfile profile)
        {
            throw new NotImplementedException();
        }
    }
}
