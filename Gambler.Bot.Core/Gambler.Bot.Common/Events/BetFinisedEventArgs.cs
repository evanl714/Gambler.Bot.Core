using Gambler.Bot.Common.Games;
using System;

namespace Gambler.Bot.Common.Events
{
    public class BetFinisedEventArgs : EventArgs
    {
        public Bet NewBet { get; set; }
        public BetFinisedEventArgs(Bet Bet)
        {
            NewBet = Bet;
        }
    }
}
