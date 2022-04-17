using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class SellingBehavior : BotStateBehavior
    {
        public override BotState State => Selling;

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
