using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gambler.Bot.Core.Games
{

    public enum Games
    {
        Dice,Crash,Roulette,Plinko
    }
    
    public abstract class Bet
    {
        public decimal TotalAmount { get; set; }

        public decimal Date { get; set; }

        [NotMapped]
        public DateTime DateValue { get { return Epoch.DateFromDecimal(Date); } set { Date = Epoch.DateToDecimal(value); } }

        public string BetID { get; set; }
        public decimal Profit { get; set; }
        public long Userid { get; set; }
        public string Currency { get; set; }
        public string Guid { get; set; }
        public decimal Edge { get; set; }
        public bool IsWin { get; set; }
        public abstract bool GetWin(BaseSite Site);
        public abstract PlaceBet CreateRetry();
    }
    
    public abstract class PlaceBet
    {
        public string GUID { get; set; }
        
        public virtual decimal TotalAmount { get { return 0; } set { } }
        public decimal BetDelay { get; set; }

    }
}
