using Gambler.Bot.Common.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gambler.Bot.Common.Games
{
    public abstract class Bet
    {
        public Games Game { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal Date { get; set; }

        [NotMapped]
        public DateTime DateValue { get { return Epoch.DateFromDecimal(Date); } set { Date = Epoch.DateToDecimal(value); } }
        [Key]
        public string BetID { get; set; }
        public decimal Profit { get; set; }
        public long Userid { get; set; }
        public string Currency { get; set; }
        public string Guid { get; set; }
        public decimal Edge { get; set; }
        public bool IsWin { get; set; }
        public abstract PlaceBet CreateRetry();
        public string Site { get; set; }
    }
}
