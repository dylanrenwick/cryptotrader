using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class SellingBehavior : BotStateBehavior
    {
        public override BotState State => Selling;

        public override void Update(ICryptoExchange api, BotProfile profile)
        {
            throw new NotImplementedException();
        }
    }
}
