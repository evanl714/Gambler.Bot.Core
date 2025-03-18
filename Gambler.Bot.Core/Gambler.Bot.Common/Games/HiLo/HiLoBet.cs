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
    }
}
