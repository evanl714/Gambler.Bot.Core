using Gambler.Bot.Common.Games.Dice;
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
        public decimal Chance { get; set; }
        public decimal Result { get; set; }
        public long Nonce { get; set; }
        public string? ServerHash { get; set; }
        public string? ServerSeed { get; set; }
        public string? ClientSeed { get; set; }
        public int WinnableType { get; set; }
        public override PlaceBet CreateRetry()
        {
            return new PlaceLimboBet(TotalAmount, Chance);
        }
        
        public override bool GetWin(IGameConfig config)
        {
            return Result >= Chance;
        }

        public override string ToCSV(IGameConfig gamecofig, long TotalBetsPlaced, decimal Balance)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}"
                        , TotalBetsPlaced, Result, Chance, "N/A", GetWin(gamecofig) ? "win" : "lose", TotalAmount, Profit, Balance, Profit);
        }
    }
}
