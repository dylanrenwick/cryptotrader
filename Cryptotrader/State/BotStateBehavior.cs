using System.Text.Json;

using Cryptotrader.Api;
using Cryptotrader.Config;
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

        public abstract Task Update(ICryptoExchange api, BotProfile profile);

        public virtual void ReadFromJson(ref Utf8JsonReader reader) { }
        public virtual void WriteToJson(Utf8JsonWriter writer)
        {
            writer.WriteString("state", Enum.GetName(typeof(BotState), State));
        }

        public static BotStateBehavior BehaviorFromBotState(BotState state)
        {
            return state switch
            {
                Startup => new StartupBehavior(),
                WaitingToSell => new WaitingToSellBehavior(),
                Selling => new SellingBehavior(),
                WaitingToBuy => new WaitingToBuyBehavior(),
                Buying => new BuyingBehavior(),
                _ => null,
            };
        }
    }
}
