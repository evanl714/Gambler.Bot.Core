using System;
using System.Linq;

namespace Gambler.Bot.Common.Games.Limbo
{
    public class LimboBet : Bet
    {
        public LimboBet()
        {
            Game = Games.Limbo;
        }
        public decimal Payout { get; set; }
        public decimal Result { get; set; }
        public long Nonce { get; set; }
        public string? ServerHash { get; set; }
        public string? ServerSeed { get; set; }
        public string? ClientSeed { get; set; }
        public int WinnableType { get; set; }
        public override PlaceBet CreateRetry()
        {
            return new PlaceLimboBet(TotalAmount, Payout);
        }
        public bool GetWin()
        {
            return Result >= Payout;
        }
    }
}
