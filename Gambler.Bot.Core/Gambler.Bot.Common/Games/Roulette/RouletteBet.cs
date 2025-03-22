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


    }

    public class PlaceRouletteBet : PlaceBet
    {
    }

    public interface iRoulette
    {
        Task<RouletteBet> PlaceRouletteBet(PlaceRouletteBet BetDetails);
    }
}
