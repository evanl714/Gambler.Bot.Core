using System;

namespace Gambler.Bot.Common.Games.Dice
{
    public interface iDice
    {
        Task<DiceBet> PlaceDiceBet(PlaceDiceBet BetDetails);
        DiceConfig DiceSettings { get; set; }

    }

    public class DiceResult:IGameResult
    {
        public decimal Roll { get; set; }

        public Games Game{ get => Games.Dice; }
    }
}
