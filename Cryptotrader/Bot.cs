using System;
using System.Threading.Tasks;

using Cryptotrader.Logging;
using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        private DateTime lastStateChange;

        private BotStateBehavior initialState;
        private BotStateBehavior activeState;

        private Logger log;

        private int updateInterval;

        public Bot(Logger logger)
        {
            log = logger;
        }

        public async Task Startup()
        {
            if (initialState == null) throw new InvalidOperationException("Initial State not set!");
            SetState(initialState);
            
            await RunBot();
        }

        public void LoadConfig(BotConfig config)
        {
            updateInterval = config.UpdateInterval;
        }

        private async Task RunBot()
        {
            while (activeState != null)
            {
                var rateLimit = Task.Delay(TimeSpan.FromSeconds(updateInterval));

                Update();

                await rateLimit;
            }
        }

        private void Update()
        {
            activeState.Update();
        }

        private void SetState(BotStateBehavior newState)
        {
            activeState?.ExitState();
            activeState = newState;
            activeState.EnterState(this);
            lastStateChange = DateTime.Now;
        }
    }
}
