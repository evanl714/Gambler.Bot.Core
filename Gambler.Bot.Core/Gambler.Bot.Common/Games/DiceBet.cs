using Gambler.Bot.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Common.Games
{
    public class DiceBet:Bet
    {
       
        public decimal Roll { get; set; }
        public bool High { get; set; }
        public decimal Chance { get; set; }
        public long Nonce { get; set; }
        public string? ServerHash { get; set; }
        public string? ServerSeed { get; set; }
        public string ClientSeed { get; set; }
        public int WinnableType { get; set; }
        public override PlaceBet CreateRetry()
        {
            return new PlaceDiceBet(TotalAmount, High, Chance);
        }

       public bool GetWin(decimal maxRoll)
        {
            return (((bool)High ? (decimal)Roll > (decimal)maxRoll - (decimal)(Chance) : (decimal)Roll < (decimal)(Chance)));
        }

        public int CalculateWinnableType(decimal maxroll)
        {
            if (Chance>=50 && Roll > maxroll - Chance && Roll < Chance)
            {
                WinnableType = 1;
            }
            else if (Chance < 50 && Roll < maxroll - Chance && Roll > Chance)
            {
                WinnableType = 2;
            }
            else if (IsWin)
            {
                WinnableType = 3;
            }
            else
            {
                WinnableType = 4;
            }
            //check if roll is between overlap
            //else if chance <50% check if roll is between non overlap
            //else if win
            //else if loss
            return WinnableType;
        }
    }
}
