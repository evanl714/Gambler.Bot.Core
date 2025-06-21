using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Common.Games.HiLo
{
    public interface iHiLo
    {
    }
    public class HiLoResult : IGameResult
    {
        public bool High { get; set; }

        public Games Game { get => Games.Dice; }
    }
}
