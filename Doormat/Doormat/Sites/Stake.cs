using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DoormatCore.Sites
{
    public class Stake:BaseSite, iDice
    {
        protected string URL = "https://primedice.com/_api/graphql";
        protected string RolName = "primediceRoll";
        protected string GameName = "CasinoGamePrimedice";
        protected string StatGameName = "primedice";
        HttpClient Client;

        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();

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
            this.DiceBetURL = "https://stake.com/bet/{0}";
            this.Edge = 2;
            URL = "https://stake.com/_api/graphql";
            RolName = "diceRoll";
            GameName = "BetGameDice";
            StatGameName = "dice";
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
            async Task OnWSConnected(GraphQL.Client.Http.GraphQLHttpClient client)
            {

            }


            public class PersonAndFilmsResponse
            {
            }


            protected override void _Login(LoginParamValue[] LoginParams)
            {
                try
                {
                    string APIKey = "";

                    foreach (LoginParamValue x in LoginParams)
                    {
                        if (x.Param.Name.ToLower() == "api key")
                            APIKey = x.Value;

                    }
                    //CookieContainer cookies = new CookieContainer();
                    var cookies = CallBypassRequired(SiteURL);

                    HttpClientHandler handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        UseCookies = true,
                        CookieContainer = cookies.Cookies,

                    };
                    Client = new HttpClient(handler);

                    Client.DefaultRequestHeaders.Add("referrer", SiteURL);
                    Client.DefaultRequestHeaders.Add("accept", "*/*");
                    Client.DefaultRequestHeaders.Add("origin", SiteURL);
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(cookies.UserAgent);
                    Client.DefaultRequestHeaders.Add("x-access-token", APIKey);
                    Client.DefaultRequestHeaders.Add("authorization", "Bearer " + APIKey);
                    Client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                    Client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                    Client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");

                    GraphqlRequestPayload LoginReq = new GraphqlRequestPayload
                    {
                        query = "query DiceBotLogin{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses betAmount profit currency}}}"
                            ,
                        operationName = "DiceBotLogin"
                    };

                    StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(LoginReq), Encoding.UTF8, "application/json");

                    var resp = Client.PostAsync(URL, content).Result;
                    string respostring = resp.Content.ReadAsStringAsync().Result;
                    var Resp = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(respostring);
                    pdUser user = Resp.data.user;
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
                                this.Stats.Profit = x.profit ?? 0;
                                this.Stats.Wagered = x.amount ?? 0;

                                break;
                            }
                        }
                        foreach (Balance x in user.balances)
                        {
                            if (x.available.currency.ToLower() == Currencies[Currency].ToLower())
                            {
                                this.Stats.Balance = x.available.amount ?? 0;
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

            public void PlaceDiceBet(PlaceDiceBet BetDetails)
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

                    //string query = "mutation {" + RolName + "(amount:" + amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) + ", target:" + tmpchance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) + ",condition:" + (High ? "above" : "below") + ",currency:" + Currencies[Currency].ToLower() + ") { id iid nonce currency amount payout state { ... on " + GameName + " { result target condition } } createdAt serverSeed{seedHash seed nonce} clientSeed{seed} user{balances{available{amount currency}} statistic{game bets wins losses amount profit currency}}}}";
                    //var primediceRoll = GQLClient.SendMutationAsync<dynamic>(new GraphQLRequest { Query = query }).Result;
                    GraphqlRequestPayload betresult = new GraphqlRequestPayload
                    {
                        query = "mutation DiceRoll($amount: Float! \r\n  $target: Float!\r\n  $condition: CasinoGameDiceConditionEnum!\r\n  $currency: CurrencyEnum!\r\n  $identifier: String!){ diceRoll(amount: $amount, target: $target, condition: $condition, currency: $currency, identifier: $identifier) { id payoutMultiplier amountMultiplier nonce currency amount payout state { ... on CasinoGameDice { result target condition } } createdAt serverSeed{seedHash seed nonce} clientSeed{seed} user{balances{available{amount currency}} statistic{game bets wins losses betAmount profit currency}}}}",
                        variables = new
                        {
                            amount = amount,
                            target = tmpchance,
                            condition = (High ? "above" : "below"),
                            currency = Currencies[base.Currency].ToLower(),
                            identifier = R.Next().ToString()
                        }
                        ,
                        operationName = "DiceRoll"
                    };
                    var response = Client.PostAsync(URL, new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(betresult), Encoding.UTF8, "application/json")).Result;
                    var responsestring = response.Content.ReadAsStringAsync().Result;
                    RollDice tmp = System.Text.Json.JsonSerializer.Deserialize<Payload>(responsestring).data.diceRoll;

                    Lastbet = DateTime.Now;
                    try
                    {

                        lastupdate = DateTime.Now;
                        /*foreach (Statistic x in tmp.user?.statistic)
                        {
                            if (x.currency.ToLower() == Currencies[Currency].ToLower() && x.game == StatGameName)
                            {*/
                        DiceBet tmpbet = tmp.ToBet();
                        tmpbet.IsWin = tmpbet.GetWin(this);
                        this.Stats.Bets++; ;
                        this.Stats.Wins += tmpbet.IsWin ? 1 : 0; ;
                        this.Stats.Losses += tmpbet.IsWin ? 0 : 1; ;
                        this.Stats.Profit += tmpbet.Profit;
                        this.Stats.Wagered += tmpbet.TotalAmount;

                        /*}
                    }*/
                        /*foreach (Balance x in tmp.user.balances)
                        {
                            if (x.available.currency.ToLower() == Currencies[Currency].ToLower())
                            {*/
                        this.Stats.Balance += tmpbet.Profit;
                        /*break;
                    }
                }*/


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

                    GraphqlRequestPayload LoginReq = new GraphqlRequestPayload
                    {
                        operationName = "DiceBotGetBalance",    
                        query = "query DiceBotGetBalance{user { activeServerSeed { seedHash seed nonce } activeClientSeed { seed } id balances { available { currency amount } } } }"
                    };
                    var Resp = Client.PostAsync("", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(LoginReq), Encoding.UTF8, "application/json")).Result;
                    string respostring = Resp.Content.ReadAsStringAsync().Result;
                    pdUser user = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(respostring)?.data.user;
                    //GraphQLResponse< pdUser> Resp = GQLClient.SendMutationAsync< pdUser>(LoginReq).Result;

                    foreach (Statistic x in user.statistic)
                    {
                        if (x.currency.ToLower() == Currencies[Currency].ToLower() && x.game == StatGameName)
                        {
                            this.Stats.Bets = (int)x.bets;
                            this.Stats.Wins = (int)x.wins;
                            this.Stats.Losses = (int)x.losses;
                            this.Stats.Profit = x.profit ?? 0;
                            this.Stats.Wagered = x.amount ?? 0;
                            break;
                        }
                    }
                    foreach (Balance x in user.balances)
                    {
                        if (x.available.currency.ToLower() == Currencies[Currency].ToLower())
                        {
                            this.Stats.Balance = x.available.amount ?? 0;
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
                return 100 - (int)(DateTime.Now - Lastbet).TotalMilliseconds;

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
                public pdUser User { get; set; }
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
                public RollDice primediceRoll { get; set; }
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
                        BetID = id.ToString(),
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
            public class GraphqlRequestPayload
            {
                public string operationName { get; set; }

                public string query { get; set; }

                public object variables { get; set; }

                public string identifier { get; set; }
            }
            public class Statistic
            {
                public string game { get; set; }
                public decimal? bets { get; set; }
                public decimal? wins { get; set; }
                public decimal? losses { get; set; }
                public decimal? amount { get; set; }
                public decimal? profit { get; set; }
                public string currency { get; set; }
                public string __typename { get; set; }
            }

            public class Data
            {
                public ChatMessages chatMessages { get; set; }
                public Messages messages { get; set; }
                public RollDice diceRoll { get; set; }
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
                public decimal? amount { get; set; }
                public string currency { get; set; }
                public string __typename { get; set; }
            }
        }

    }


