using System;

namespace Gambler.Bot.Common.Games
{
    public interface iDice
    {
        Task<DiceBet> PlaceDiceBet(PlaceDiceBet BetDetails);
    }
}
