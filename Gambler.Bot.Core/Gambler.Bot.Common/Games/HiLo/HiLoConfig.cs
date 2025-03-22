using Gambler.Bot.Common.Games.Dice;
using System;
using System.Linq;

namespace Gambler.Bot.Common.Games.HiLo
{
    public class HiLoConfig : IGameConfig
    {
        public decimal Edge { get; set; }
    }
}
