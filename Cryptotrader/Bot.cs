using System;

using Cryptotrader.Logging;
using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        private DateTime lastStateChange;

        private BotStateBehavior initialState;
        private BotStateBehavior activeState;

        private Logger logger;

        public Bot(Logger log)
        {
            logger = log;
        }

        public void Startup()
        {
            if (initialState == null) throw new InvalidOperationException("Initial State not set!");
            SetState(initialState);
        }

        public void Update()
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
