using System;
using System.Collections.Generic;
using System.Text;

using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class BotStateBehavior
    {
        public Bot Bot { get; private set; }

        public virtual void EnterState(Bot bot)
        {
            Bot = bot;
        }
        public virtual void ExitState() { }

        public virtual void Update() { }

        public virtual BotState State => None;

        public static BotStateBehavior BehaviorFromBotState(BotState state)
        {
            switch(state)
            {
                case Startup:
                    return new StartupBehavior();
                case WaitingToSell:
                    return new WaitingToSellBehavior();
                case Selling:
                    return new SellingBehavior();
                case WaitingToBuy:
                    return new WaitingToBuyBehavior();
                case Buying:
                    return new BuyingBehavior();
                case None:
                default:
                    return new BotStateBehavior();
            }
        }
    }
}
