using System;
using System.Collections.Generic;
using System.Text;

using Cryptotrader.State;

namespace Cryptotrader
{
    public class Bot
    {
        private DateTime lastStateChange;

        private BotStateBehavior initialState;
        private BotStateBehavior activeState;

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
            activeState.EnterState();
            lastStateChange = DateTime.Now;
        }
    }
}
