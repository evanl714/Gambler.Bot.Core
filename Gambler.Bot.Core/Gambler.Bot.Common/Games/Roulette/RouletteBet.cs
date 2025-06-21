using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Common.Games.Roulette
{
    public class RouletteBet : Bet
    {
        public RouletteBet()
        {
            Game = Games.Roulette;
        }
        public override PlaceBet CreateRetry()
        {
            throw new NotImplementedException();
        }

        public override bool GetWin(IGameConfig config)
        {
            throw new NotImplementedException();
        }

        public override string ToCSV(IGameConfig gamecofig, long TotalBetsPlaced, decimal Balance)
        {
            throw new NotImplementedException();
        }
    }

    public class PlaceRouletteBet : PlaceBet
    {
    }

    public interface iRoulette
    {
        Task<RouletteBet> PlaceRouletteBet(PlaceRouletteBet BetDetails);
    }
}
