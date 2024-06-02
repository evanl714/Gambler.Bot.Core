using Gambler.Bot.Core.Games;
using System;

namespace Gambler.Bot.Core.Events
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
