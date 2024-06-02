using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gambler.Bot.Core.Sites;

namespace Gambler.Bot.Core.Games
{
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class CrashBet : Bet
    {
        public decimal Payout { get; set; }
        public decimal Crash { get; set; }

        public override PlaceBet CreateRetry()
        {
            return new PlaceCrashBet { Payout = Payout, TotalAmount = TotalAmount };
        }

        public override bool GetWin(BaseSite Site)
        {
            throw new NotImplementedException();
        }
    }
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class PlaceCrashBet : PlaceBet
    {
        public decimal Payout { get; set; }
    }
    public interface iCrash
    {
        Task<CrashBet> PlaceCrashBet(PlaceCrashBet BetDetails);
    }
}
