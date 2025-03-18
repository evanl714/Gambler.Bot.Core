using Gambler.Bot.Common.Games.Dice;
using System;

namespace Gambler.Bot.Common.Games.Crash
{
    public class CrashConfig : IGameConfig
    {
        public decimal Edge { get; set; }
        public bool IsMultiplayer { get; set; } = true;
    }
}
