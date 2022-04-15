using System;
using System.Text.Json;

using Cryptotrader.Logging;

using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public abstract class BotStateBehavior
    {
        public Bot Bot { get; private set; }

        protected Logger log;

        public virtual void EnterState(Bot bot, Logger logger)
        {
            Bot = bot;
            log = logger;
        }
        public virtual void ExitState() { }

        public abstract BotState State { get; }

        public abstract void Update();

        public virtual void ReadFromJson(ref Utf8JsonReader reader) { }
        public virtual void WriteToJson(Utf8JsonWriter writer)
        {
            writer.WriteString("state", Enum.GetName(typeof(BotState), State));
        }

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
                    return null;
            }
        }
    }
}
