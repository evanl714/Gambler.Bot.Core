namespace Gambler.Bot.Common.Games
{
    public abstract class PlaceBet
    {
        public string GUID { get; set; }

        public virtual decimal TotalAmount { get { return 0; } set { } }
        public decimal BetDelay { get; set; }

    }
}
