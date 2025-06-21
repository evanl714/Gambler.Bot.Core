using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Common.Games.Plinko
{

    public class PlinkoBet : Bet
    {
        public PlinkoBet()
        {
            Game = Games.Plinko;
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
}
