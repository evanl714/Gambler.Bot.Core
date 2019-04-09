using DoormatCore.Sites;
using DoormatCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore.Games
{
    [MoonSharp.Interpreter.MoonSharpUserData]
    [PersistentTableName("DICEBET")]
    public class DiceBet:Bet
    {
       
        public decimal Roll { get; set; }
        public bool High { get; set; }
        public decimal Chance { get; set; }
        public long Nonce { get; set; }
        public string ServerHash { get; set; }
        public string ServerSeed { get; set; }
        public string ClientSeed { get; set; }

        public override PlaceBet CreateRetry()
        {
            return new PlaceDiceBet(TotalAmount, High, Chance);
        }

        public override bool GetWin(BaseSite Site)
        {
            return (((bool)High ? (decimal)Roll > (decimal)Site.MaxRoll - (decimal)(Chance) : (decimal)Roll < (decimal)(Chance)));
        }
    }
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class PlaceDiceBet: PlaceBet
    {
        /// <summary>
        /// Amount to be bet
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Bet high when true, low when false
        /// </summary>
        public bool High { get; set; }
        /// <summary>
        /// The chance to place the bet at
        /// </summary>
        public decimal Chance { get; set; }

        /// <summary>
        /// The unique internal identifier to detect duplicate or skipped bets.
        /// </summary>
        

        public PlaceDiceBet(decimal Amount, bool High, decimal Chance, string Guid)
        {
            this.Amount = Amount;
            this.High = High;
            this.Chance = Chance;
            this.GUID = GUID;
        }
        public PlaceDiceBet(decimal Amount, bool High, decimal Chance)
        {
            this.Amount = Amount;
            this.High = High;
            this.Chance = Chance;

        }
    }
}
