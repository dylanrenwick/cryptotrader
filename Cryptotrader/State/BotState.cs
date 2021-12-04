﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptotrader.State
{
    public enum BotState
    {
        None = 0,
        Startup,
        WaitingToSell,
        Selling,
        WaitingToBuy,
        Buying
    }
}
