using DoormatCore.Sites;
using DoormatCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore.Games
{

    public enum Games
    {
        Dice,Crash,Roulette,Plinko
    }
    [MoonSharp.Interpreter.MoonSharpUserData]
    public abstract class Bet:PersistentBase
    {
        public decimal TotalAmount { get; set; }

        public decimal Date { get; set; }

        [NonPersistent]
        public DateTime DateValue { get { return SQLBase.DateFromDecimal(Date); } set { Date = SQLBase.DateToDecimal(value); } }

        public string BetID { get; set; }
        public decimal Profit { get; set; }
        public long Userid { get; set; }
        public string Currency { get; set; }
        public string Guid { get; set; }

        public abstract bool GetWin(BaseSite Site);
        public abstract PlaceBet CreateRetry();
    }
    [MoonSharp.Interpreter.MoonSharpUserData]
    public abstract class PlaceBet
    {
        public string GUID { get; set; }
        [NonPersistent]
        public virtual decimal TotalAmount { get { return 0; } set { } }

    }
}
