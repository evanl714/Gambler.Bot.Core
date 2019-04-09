using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace DoormatCore.Sites
{
    public class PrimeDice : BaseSite
    {
        protected string URL = "https://api.primedice.com/graphql";
        protected string RolName = "primediceRoll";
        protected string GameName = "BetGamePrimedice";
        protected string CaptchaKey = "6LdXCWoUAAAAAEiWih-AFu1G-Uqnslks1v0-4pVv";
        protected string StatGameName = "primedice";

        GraphQL.Client.GraphQLClient GQLClient = new GraphQL.Client.GraphQLClient("https://api.primedice.com/graphql");
       
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        public PrimeDice()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", true, true, false, true) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "PD";
            this.SiteName = "PrimeDice";
            this.SiteURL = "https://primedice.com";
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
            this.Edge = 1;
            
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            ispd = false;
        }
        string userid = "";
        protected override void _Login(LoginParamValue[] LoginParams)
        {
            try
            {
                string APIKey = "";
                GQLClient = new GraphQL.Client.GraphQLClient(URL);
                foreach (LoginParamValue x in LoginParams)
                {
                    if (x.Param.Name.ToLower() == "api key")
                        APIKey = x.Value;
                    
                }
                
                GQLClient.DefaultRequestHeaders.Add("x-access-token", APIKey);
                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "query{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses amount profit currency}}}"
                };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                pdUser user = Resp.GetDataFieldAs<pdUser>("user");                
                userid = user.id;
                if (string.IsNullOrWhiteSpace(userid))
                    callLoginFinished(false);
                else
                {
                    foreach (Statistic x in user.statistic)
                    {
                        if (x.currency.ToLower() == Currencies[Currency].ToLower() && x.game == StatGameName)
                        {
                            this.Stats.Bets = (int)x.bets;
                            this.Stats.Wins = (int)x.wins;
                            this.Stats.Losses = (int)x.losses;
                            this.Stats.Profit = x.profit;
                            this.Stats.Wagered = x.amount;
                            
                            break;
                        }
                    }
                    foreach (Balance x in user.balances)
                    {
                        if (x.available.currency.ToLower() == Currencies[Currency].ToLower())
                        {
                            this.Stats.Balance  = x.available.amount;
                            break;
                        }
                    }

                    callLoginFinished(true);
                    return;
                }

            }
            catch (WebException e)
            {
                Logger.DumpLog(e);
                if (e.Response != null)
                {
                    
                }
                callLoginFinished(false);
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                callLoginFinished(false);
            }
        }
        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (userid != null && ((DateTime.Now - lastupdate).TotalSeconds >= 30 || ForceUpdateStats))
                    {
                        UpdateStats();
                        
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
            }
        }

        int retrycount = 0;
        DateTime Lastbet = DateTime.Now;

        protected override void _PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                decimal amount = BetDetails.Amount;
                decimal chance = BetDetails.Chance;
                bool High = BetDetails.High;
                /*if (amount < 10000 && (DateTime.Now - Lastbet).TotalMilliseconds < 500)
                {
                    Thread.Sleep((int)(500.0 - (DateTime.Now - Lastbet).TotalMilliseconds));
                }*/
                decimal tmpchance = High ? MaxRoll - chance : chance;

                string query = "mutation{" + RolName + "(amount:" + amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) + ", target:" + tmpchance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) + ",condition:" + (High ? "above" : "below") + ",currency:" + Currencies[Currency].ToLower() + ") { id iid nonce currency amount payout state { ... on " + GameName + " { result target condition } } createdAt serverSeed{seedHash seed nonce} clientSeed{seed} user{balances{available{amount currency}} statistic{game bets wins losses amount profit currency}}}}";
                GraphQLResponse betresult = GQLClient.PostAsync(new GraphQLRequest { Query = query}).Result;
                RollDice tmp = betresult.GetDataFieldAs<RollDice>(RolName);


                Lastbet = DateTime.Now;
                try
                {

                    lastupdate = DateTime.Now;
                    foreach (Statistic x in tmp.user.statistic)
                    {
                        if (x.currency.ToLower() == Currencies[Currency].ToLower() && x.game == StatGameName)
                        {
                            this.Stats.Bets = (int)x.bets;
                            this.Stats.Wins = (int)x.wins;
                            this.Stats.Losses = (int)x.losses;
                            this.Stats.Profit = x.profit;
                            this.Stats.Wagered = x.amount;
                            break;
                        }
                    }
                    foreach (Balance x in tmp.user.balances)
                    {
                        if (x.available.currency.ToLower() == Currencies[Currency].ToLower())
                        {
                            this.Stats.Balance = x.available.amount;
                            break;
                        }
                    }
                    DiceBet tmpbet = tmp.ToBet();
                    tmpbet.Guid = BetDetails.GUID;
                    callBetFinished(tmpbet);
                    retrycount = 0;
                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                    callNotify("Some kind of error happened. I don't really know graphql, so your guess as to what went wrong is as good as mine.");
                }
            }
            catch (Exception e2)
            {
                callNotify("Error occured while trying to bet, retrying in 30 seconds. Probably.");
                Logger.DumpLog(e2);
            }
        }

        protected override void _UpdateStats()
        {
            try
            {
                ForceUpdateStats = false;
                lastupdate = DateTime.Now;
                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "query{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses amount profit currency}}}"
                };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                pdUser user = Resp.GetDataFieldAs<pdUser>("user");
                foreach (Statistic x in user.statistic)
                {
                    if (x.currency.ToLower() == Currencies[Currency].ToLower() && x.game == StatGameName)
                    {
                        this.Stats.Bets = (int)x.bets;
                        this.Stats.Wins = (int)x.wins;
                        this.Stats.Losses = (int)x.losses;
                        this.Stats.Profit = x.profit;
                        this.Stats.Wagered = x.amount;
                        break;
                    }
                }
                foreach (Balance x in user.balances)
                {
                    if (x.available.currency.ToLower() == Currencies[Currency].ToLower())
                    {
                        this.Stats.Balance = x.available.amount;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
            }
        }

        public override int _TimeToBet(PlaceBet BetDetails)
        {
            return 1000-(int)(DateTime.Now - Lastbet).TotalMilliseconds;

        }


        public class Sender
        {
            public string name { get; set; }
            public string __typename { get; set; }
        }

        public class Receiver
        {
            public string name { get; set; }
            public string __typename { get; set; }
        }

        public class _Tip
        {
            public string id { get; set; }
            public decimal amount { get; set; }
            public string currency { get; set; }
            public Sender sender { get; set; }
            public Receiver receiver { get; set; }
            public string __typename { get; set; }
        }
        public class Data2
        {
            public string message { get; set; }
            public _Tip tip { get; set; }
            public string __typename { get; set; }
        }
        public class pdSeed
        {
            public string seedHash { get; set; }
            public string seed { get; set; }
            public int nonce { get; set; }
        }
        public class pdUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<object> roles { get; set; }
            public string __typename { get; set; }
            public Balance balance { get; set; }
            public Balance[] balances { get; set; }
            public List<Statistic> statistic { get; set; }
            public pdSeed activeSeed { get; set; }
        }

        public class ChatMessages
        {
            public string id { get; set; }
            public Data2 data { get; set; }
            public string createdAt { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
        }
        public class Chat
        {
            public string id { get; set; }
            public string __typename { get; set; }
        }
        public class Messages
        {
            public Chat chat { get; set; }
            public string id { get; set; }
            public Data2 data { get; set; }
            public string createdAt { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
        }
        public class DiceState
        {
            public double result { get; set; }
            public double target { get; set; }
            public string condition { get; set; }

        }
        public class RollDice
        {
            public string id { get; set; }
            public string iid { get; set; }
            public decimal result { get; set; }
            public decimal payoutMultiplier { get; set; }
            public decimal amount { get; set; }
            public decimal payout { get; set; }
            public string createdAt { get; set; }
             public string currency { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
            public pdSeed serverSeed { get; set; }
            public pdSeed clientSeed { get; set; }
            public int nonce { get; set; }
            public DiceState state { get; set; }

            public DiceBet ToBet()
            {
                DiceBet bet = new DiceBet
                {
                    TotalAmount = amount,
                    Chance = state.condition.ToLower() == "above" ? 99.99m - (decimal)state.target : (decimal)state.target,
                    High = state.condition.ToLower() == "above",
                    Currency = currency,
                    DateValue = DateTime.Now,
                    BetID = iid.ToString(),
                    Roll = (decimal)state.result,
                    ClientSeed = clientSeed.seed,
                    ServerHash = serverSeed.seedHash,
                    Nonce = nonce                    
                };
                
                //User tmpu = User.FindUser(bet.UserName);
                /*if (tmpu == null)
                    bet.uid = 0;
                else
                    bet.uid = (int)tmpu.Uid;*/
                bool win = (((bool)bet.High ? (decimal)bet.Roll > (decimal)99.99 - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
                bet.Profit = win ? ((payout - amount)) : (-amount);
                return bet;
            }
        }
        public class Statistic
        {
            public string game { get; set; }
            public decimal bets { get; set; }
            public decimal wins { get; set; }
            public decimal losses { get; set; }
            public decimal amount { get; set; }
            public decimal profit { get; set; }
            public string currency { get; set; }
            public string __typename { get; set; }
        }

        public class Data
        {
            public ChatMessages chatMessages { get; set; }
            public Messages messages { get; set; }
            public RollDice rollDice { get; set; }
            public pdUser user { get; set; }
            public RollDice bet { get; set; }
        }

        public class Payload
        {
            public Data data { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public string id { get; set; }
            public Payload payload { get; set; }
        }
        public class Role
        {
            public string name { get; set; }
            public string __typename { get; set; }
        }
        public class Balance
        {
            public Available available { get; set; }
            public string __typename { get; set; }
        }
        public class Available
        {
            public decimal amount { get; set; }
            public string currency { get; set; }
            public string __typename { get; set; }
        }
    }
    
}
