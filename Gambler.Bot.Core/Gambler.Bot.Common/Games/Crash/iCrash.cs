using System;
using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Common.Games.Crash
{
    public interface iCrash
    {
        Task<CrashBet> PlaceCrashBet(PlaceCrashBet BetDetails);
        CrashConfig CrashSettings { get; set; }
    }
    public class CrashResult : IGameResult
    {
        public decimal Result { get; set; }

        public Games Game { get => Games.Dice; }
    }
}
