using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore.Sites
{
    public class Stake:PrimeDice
    {
        public Stake()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", true, true, false, true) };
            this.MaxRoll = 100m;
            this.SiteAbbreviation = "ST";
            this.SiteName = "Stake";
            this.SiteURL = "https://Stake.com";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = true;
            this.CanChangeSeed = true;
            this.CanChat = false;
            this.CanGetSeed = true;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = true;
            this.CanVerify = true;
            this.Currencies = new string[] { "Btc", "Ltc", "Eth", "Doge", "Bch" };
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://primedice.com/bet/{0}";
            this.Edge = 2;
            URL = "https://api.stake.com/graphql/";
            RolName = "diceRoll";
            GameName = "BetGameDice";
            StatGameName = "dice";
        }
    }
}
