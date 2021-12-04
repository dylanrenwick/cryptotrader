using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class WaitingToSellBehavior : BotStateBehavior
    {
        public override BotState State => WaitingToSell;
    }
}
