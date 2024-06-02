using System;

namespace Gambler.Bot.Common.Games
{
    public interface iCrash
    {
        Task<CrashBet> PlaceCrashBet(PlaceCrashBet BetDetails);
    }
}
