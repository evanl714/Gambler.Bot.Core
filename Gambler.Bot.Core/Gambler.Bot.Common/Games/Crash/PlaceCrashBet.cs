using System;

namespace Gambler.Bot.Common.Games.Crash
{
    public class PlaceCrashBet : PlaceBet
    {
        public decimal Payout { get; set; }
        public PlaceCrashBet(decimal payout, decimal amount)
        {
            Amount = amount;
            Payout = payout;
            Game = Games.Crash;
        }
    }
}
