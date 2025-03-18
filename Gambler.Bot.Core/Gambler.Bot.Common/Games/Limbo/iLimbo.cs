using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Common.Games.Limbo
{
    public interface iLimbo
    {
        public Task<LimboBet> PlaceLimboBet(PlaceLimboBet bet);
        public LimboConfig LimboSettings { get; set; }
    }
}
