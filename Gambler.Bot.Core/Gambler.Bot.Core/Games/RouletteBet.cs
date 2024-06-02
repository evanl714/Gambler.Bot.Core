using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gambler.Bot.Core.Sites;

namespace Gambler.Bot.Core.Games
{
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class RouletteBet : Bet
    {
        public override PlaceBet CreateRetry()
        {
            throw new NotImplementedException();
        }

        public override bool GetWin(BaseSite Site)
        {
            throw new NotImplementedException();
        }
    }

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class PlaceRouletteBet:PlaceBet
    {

    }

    public interface iRoulette
    {
        Task<RouletteBet> PlaceRouletteBet(PlaceRouletteBet BetDetails);
    }
}
