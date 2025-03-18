using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Common.Games.Crash
{
    public class CrashBet : Bet
    {
        public decimal Payout { get; set; }
        public decimal Crash { get; set; }
        public long Nonce { get; set; }
        public string? ServerHash { get; set; }
        public string? ServerSeed { get; set; }


        public override PlaceBet CreateRetry()
        {
            return new PlaceCrashBet { Payout = Payout, TotalAmount = TotalAmount };
        }


    }
}
