using Cryptotrader.Api;
using Cryptotrader.Config;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class StartupBehavior : BotStateBehavior
    {
        public override BotState State => Startup;

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            throw new NotImplementedException();
        }
    }
}
