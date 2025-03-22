namespace Gambler.Bot.Common.Helpers
{
    public class SiteStats
    {
        public Currency Currency { get; set; }
        public decimal Balance { get; set; }
        public Games.Games Game { get; set; }
        public decimal Wagered { get; set; }
        public decimal Profit { get; set; }
        public long Bets { get; set; }
        public long Wins { get; set; }
        public long Losses { get; set; }
    }
}
