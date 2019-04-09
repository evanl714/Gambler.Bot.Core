using System;
using System.Collections.Generic;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;

namespace DoormatCore.Storage
{
    class Mongo : SQLBase
    {
        public Mongo(string ConnectionString) : base(ConnectionString)
        {
            Logger.DumpLog("Create Mongo Connection", 6);
        }

        public override SessionStats AddSessionStats(SessionStats Stats)
        {
            throw new NotImplementedException();
        }
        
        public override DiceBet GetBet(int InternalId)
        {
            throw new NotImplementedException();
        }

        public override DiceBet GetBet(string BetId)
        {
            throw new NotImplementedException();
        }

        public override DiceBet[] GetBets()
        {
            throw new NotImplementedException();
        }

        public override DiceBet[] GetBets(Site Site)
        {
            throw new NotImplementedException();
        }

        public override DiceBet[] GetBets(User User)
        {
            throw new NotImplementedException();
        }

        public override string GetConnectionString()
        {
            throw new NotImplementedException();
        }

        public override Currency[] GetCurrenciesForSite(Site Site)
        {
            throw new NotImplementedException();
        }

        public override Currency GetCurrency(int Id)
        {
            throw new NotImplementedException();
        }

        public override Currency GetCurrency(string Nameorsymbol)
        {
            throw new NotImplementedException();
        }

        public override BaseSite.LoginParameter GetParameter(int Id)
        {
            throw new NotImplementedException();
        }

        public override Seed GetSeed(int Id)
        {
            throw new NotImplementedException();
        }

        public override Seed GetSeed(string ServerSeedHash, string ServerSeed = null)
        {
            throw new NotImplementedException();
        }

        public override Seed[] GetSeeds()
        {
            throw new NotImplementedException();
        }

        public override SessionStats[] GetSessionStats()
        {
            throw new NotImplementedException();
        }

        public override SessionStats[] GetSessionStats(Site Site)
        {
            throw new NotImplementedException();
        }

        public override SessionStats[] GetSessionStats(User User)
        {
            throw new NotImplementedException();
        }

        public override Site GetSite(int Id)
        {
            throw new NotImplementedException();
        }

        public override Site GetSite(string SiteName)
        {
            throw new NotImplementedException();
        }

        public override Site GetSites(bool ActiveOnly)
        {
            throw new NotImplementedException();
        }

        public override User GetUser(int Id)
        {
            throw new NotImplementedException();
        }

        public override User GetUserById(string Id)
        {
            throw new NotImplementedException();
        }

        public override User GetUserByName(string Name)
        {
            throw new NotImplementedException();
        }

        public override DiceBet UpdateBet(DiceBet NewBet)
        {
            throw new NotImplementedException();
        }

        public override Currency UpdateCurrency(Currency NewCurrency)
        {
            throw new NotImplementedException();
        }


        public override BaseSite.LoginParameter UpdateParameter(BaseSite.LoginParameter LoginParameter)
        {
            throw new NotImplementedException();
        }

        public override Seed UpdateSeed(Seed NewSeed)
        {
            throw new NotImplementedException();
        }

        public override Site UpdateSite(Site NewSite)
        {
            throw new NotImplementedException();
        }

        public override User UpdateUser(User NewUser)
        {
            throw new NotImplementedException();
        }

        public override BaseSite.LoginParamValue UpdateValue(BaseSite.LoginParamValue NewValue)
        {
            throw new NotImplementedException();
        }

       
        protected override void CreateBetT()
        {
            throw new NotImplementedException();
        }

        protected override void CreateCurrencyT()
        {
            throw new NotImplementedException();
        }

        protected override void CreateLoginParamT()
        {
            throw new NotImplementedException();
        }

        protected override void CreateSeedT()
        {
            throw new NotImplementedException();
        }

        protected override void CreateSessionStatsT()
        {
            throw new NotImplementedException();
        }

        protected override void CreateSiteT()
        {
            throw new NotImplementedException();
        }

        protected override void CreateUserT()
        {
            throw new NotImplementedException();
        }
    }
}
