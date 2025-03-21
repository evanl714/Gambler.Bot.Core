using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Sites
{
    public class BCHGames : BaseSite, iDice, iLimbo, iCrash
    {
        public DiceConfig DiceSettings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public LimboConfig LimboSettings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CrashConfig CrashSettings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public BCHGames()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", true, true, false, true),};
            IsEnabled = false;
            //this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "BCH";
            this.SiteName = "BCH.Games";
            this.SiteURL = "https://bch.games/play/Seuntjie";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = true;
            this.CanChangeSeed = true;
            this.CanChat = false;
            this.CanGetSeed = true;
            this.CanRegister = false;
            this.CanSetClientSeed = true;
            this.CanTip = true;
            this.CanVerify = true;
            this.Currencies = new string[] { "bch" };

            
            SupportedGames = new Games[] { Games.Dice };
            CurrentCurrency = "bch";
            this.DiceBetURL = "https://bch.games/bet/{0}";
            //this.Edge = 1;
            DiceSettings = new DiceConfig() { Edge = 2, MaxRoll = 99.99m };
            LimboSettings = new LimboConfig() { Edge = 2, MinChance = 0.000098m };
            CrashSettings = new CrashConfig() { Edge = 1, IsMultiplayer = true };
            NonceBased = true;
        }

        public Task<DiceBet> PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            throw new NotImplementedException();
        }

        public Task<LimboBet> PlaceLimboBet(PlaceLimboBet bet)
        {
            throw new NotImplementedException();
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> _Login(LoginParamValue[] LoginParams)
        {
            throw new NotImplementedException();
        }

        protected override Task<SiteStats> _UpdateStats()
        {
            throw new NotImplementedException();
        }

        public Task<CrashBet> PlaceCrashBet(PlaceCrashBet BetDetails)
        {
            throw new NotImplementedException();
        }
    }
}

