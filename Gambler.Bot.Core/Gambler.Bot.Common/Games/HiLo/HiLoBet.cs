using Gambler.Bot.Common.Games.Dice;
using System;
using System.Linq;

namespace Gambler.Bot.Common.Games.HiLo
{
    public class HiLoBet : Bet
    {
        public HiLoBet()
        {
            Game = Games.HiLo;
        }
        public override PlaceBet CreateRetry()
        {
            throw new NotImplementedException();
        }

        public override bool GetWin(IGameConfig config)
        {
            throw new NotImplementedException();
        }

        public override string ToCSV(IGameConfig gamecofig, long TotalBetsPlaced, decimal Balance)
        {
            throw new NotImplementedException();
        }
    }
}
