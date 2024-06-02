using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Common.Games
{
    public class CrashBet : Bet
    {
        public decimal Payout { get; set; }
        public decimal Crash { get; set; }

        public override PlaceBet CreateRetry()
        {
            return new PlaceCrashBet { Payout = Payout, TotalAmount = TotalAmount };
        }

        
    }
}
