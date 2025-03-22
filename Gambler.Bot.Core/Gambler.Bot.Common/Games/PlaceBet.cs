namespace Gambler.Bot.Common.Games
{
    public abstract class PlaceBet
    {
        public string GUID { get; set; }

        public Games Game { get; set; }
        public decimal BetDelay { get; set; }
        public decimal Amount { get; set; }

    }
}
