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


    }
}
