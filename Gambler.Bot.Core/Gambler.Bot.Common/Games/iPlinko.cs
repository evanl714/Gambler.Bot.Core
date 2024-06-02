namespace Gambler.Bot.Common.Games
{
    public interface iPlinko
    {
        Task<PlinkoBet> PlacePlinkoBet(PlacePlinkoBet BetDetails);
    }
}
