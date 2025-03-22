using Gambler.Bot.Common.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Events
{
    public class GameMessageEventArgs
    {
        public IGameMessage Message { get; set; }
    }
}
