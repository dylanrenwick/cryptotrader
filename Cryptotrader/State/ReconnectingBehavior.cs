using Cryptotrader.Api;
using Cryptotrader.Config;

namespace Cryptotrader.State
{
    public class ReconnectingBehavior : BotStateBehavior
    {
        public override BotState State => BotState.Reconnecting;

        private readonly BotStateBehavior lastState;
        private ulong attempts = 1;

        public ReconnectingBehavior(BotStateBehavior state)
        {
            lastState = state;
        }

        public override async Task Update(ICryptoExchange api, BotProfile profile)
        {
            if (await api.UpdatePrices())
            {
                log.Alert($"Successfully reconnected to exchange API after {attempts} attempts");
                Bot.SetState(lastState);
            }
            else if (attempts == ulong.MaxValue)
            {
                log.Exception(new Exception($"Failed to reconnect to exchange API after {attempts} attempts."));
            }
            else
            {
                log.Info($"Failed to reconnect to exchange API. Attempt {attempts}");
                attempts++;
            }
        }
    }
}
