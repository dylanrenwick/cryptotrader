using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class StartupBehavior : BotStateBehavior
    {
        public override BotState State => Startup;

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
