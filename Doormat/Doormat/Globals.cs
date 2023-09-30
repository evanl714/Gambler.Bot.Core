using System;

namespace DoormatCore
{
    public class Globals
    {
        public object SiteStats { get; set; }
        public object SiteDetails { get; set; }
        public object Stats { get; set; }
        public object NextDiceBet { get; set; }
        public object PreviousDiceBet { get; set; }
        public bool DiceWin { get; set; }
        public decimal Balance { get; set; }
        public Action<string, decimal> Withdraw { get; set; }
        public Action<decimal> Invest{ get; set; }
        public Action<string, decimal> Tip{ get; set; }
        public Action ResetSeed{ get; set; }
        public Action<string> Print{ get; set; }
        public Action<decimal, long> RunSim{ get; set; }
        public Action ResetStats{ get; set; }
        public Func<string, int, object> Read{ get; set; }
        public Func<string, int, string, string, string, object>  Readadv { get; set; }
        public Action Alarm{ get; set; }
        public Action Ching{ get; set; }
        public Action ResetBuiltIn{ get; set; }
        public Action<string> ExportSim { get; set; }
        public Action Stop { get; set; }
        public Action<string> SetCurrency { get; set; }
    }
}
