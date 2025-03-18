using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Common.Games.Plinko
{
    public interface iPlinko
    {
        Task<PlinkoBet> PlacePlinkoBet(PlacePlinkoBet BetDetails);
    }

    public class PlinkoConfig : IGameConfig
    {
        public decimal Edge { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

}
