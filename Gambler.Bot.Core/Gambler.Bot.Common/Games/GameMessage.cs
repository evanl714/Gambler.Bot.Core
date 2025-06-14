using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Common.Games
{
    public interface IGameMessage
    {
        public Games Game { get;}

        public string Message { get; }

    }

    public interface IGameResult
    {
        Games Game { get; }

    }
}
