using DoormatCore.Helpers;
using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore.Games
{

    public enum Games
    {
        Dice,Crash,Roulette,Plinko
    }
    
    public abstract class Bet:PersistentBase
    {
        public decimal TotalAmount { get; set; }

        public decimal Date { get; set; }

        [NonPersistent]
        public DateTime DateValue { get { return json.DateFromDecimal(Date); } set { Date = json.DateToDecimal(value); } }

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

    }
}
