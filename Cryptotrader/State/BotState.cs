using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptotrader.State
{
    public abstract class BotState
    {
        public Bot Bot { get; private set; }

        public BotState(Bot bot)
        {
            Bot = bot;
        }

        public abstract void EnterState();
        public abstract void ExitState();

        public abstract void Update();
    }
}
