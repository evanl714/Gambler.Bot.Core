using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;

namespace DoormatCore.Sites
{
    public class Dice999 : BaseSite, iDice
    {
        string sessionCookie = "";
        
        long uid = 0;

        bool isD999 = false;

        public static string[] cCurrencies = new string[] { "btc", "doge", "ltc", "eth" };
        HttpClientHandler ClientHandlr;
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://www.999dice.com/api/web.aspx") };
        int site = 0;
        bool Loggedin = false;
        string[] SiteA = new string[] {"https://www.999dice.com/api/web.aspx" ,
            "https://www.999proxy.com/api/web.aspx",
            "https://www.999doge.com/api/web.aspx",
            "https://www.999-dice.com/api/web.aspx",
            "http://999again.ddns.net:999/api/web.aspx" };
        public Dice999()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            this.MaxRoll = 99.9999m;
            this.SiteAbbreviation = "999";
            this.SiteName = "999Dice";
            this.SiteURL = "https://www.999dice.com/?20073598";
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
            this.Currencies = cCurrencies;
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://www.999dice.com/Bets/?b={0}";
            this.Edge = 0.1m;
        }
        void GetBalanceThread()
        {
            while (isD999)
            {
                if (sessionCookie != "" && sessionCookie != null && ((DateTime.Now - Lastbalance).TotalSeconds >= 60 || ForceUpdateStats))
                {
                    Lastbalance = DateTime.Now;
                    UpdateStats();
                }
                Thread.Sleep(1100);
            }
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            isD999 = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            try
            {
                string sitea = SiteA[site];                
                string Username = "", Password = "", twofa = "";
                foreach (LoginParamValue x in LoginParams)
                {
                    if (x.Param.Name.ToLower() == "username")
                        Username = x.Value;
                    if (x.Param.Name.ToLower() == "password")
                        Password = x.Value;
                    if (x.Param.Name.ToLower() == "2fa code")
                        twofa = x.Value;
                }
                ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,  }; ;
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri(sitea) };
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "Login"));
                pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
                if (twofa != "" && twofa != null)
                    pairs.Add(new KeyValuePair<string, string>("Totp", twofa));

                pairs.Add(new KeyValuePair<string, string>("Username", Username));
                pairs.Add(new KeyValuePair<string, string>("Password", Password));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (site++ < SiteA.Length - 1)
                            _Login(LoginParams);
                        else
                            callLoginFinished(false);
                        return;

                    }
                }

                d999Login tmpU = json.JsonDeserialize<d999Login>(responseData);
                if (tmpU.SessionCookie != "" && tmpU.SessionCookie != null)
                {
                    Lastbalance = DateTime.Now;
                    sessionCookie = tmpU.SessionCookie;
                    Stats.Balance = tmpU.Balance / 100000000.0m;
                    Stats.Profit = tmpU.Profit / 100000000.0m;
                    Stats.Wagered = tmpU.Wagered / 100000000m;
                    Stats.Bets = (int)tmpU.BetCount;
                    Stats.Wins = (int)tmpU.BetWinCount;
                    Stats.Losses = (int)tmpU.BetLoseCount;
                    Lastbalance = DateTime.Now.AddMinutes(-2);
                    UpdateStats();                    
                    uid = tmpU.Accountid;
                }
                else
                {

                }
                if (sessionCookie != "")
                {
                    isD999 = true;
                    Thread t = new Thread(GetBalanceThread);
                    t.Start();

                }
            }
            catch
            {
                if (++site < SiteA.Length)
                    _Login(LoginParams);
            }
            if (!Loggedin)
            {
                callLoginFinished(sessionCookie != "");
                //Loggedin = true;
            }
        }

        int retrycount = 0;
        string next = "";
        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            string err = "";
            try
            {
                bool High = BetDetails.High;
                decimal amount = BetDetails.Amount;
                
                decimal chance = (999999.0m) * (BetDetails.Chance / 100.0m);
                //HttpWebResponse EmitResponse;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                if (next == "" && next != null)
                {



                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("a", "GetServerSeedHash"));
                    pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));

                    Content = new FormUrlEncodedContent(pairs);
                    responseData = "";
                    using (var response = Client.PostAsync("", Content))
                    {
                        try
                        {
                            responseData = response.Result.Content.ReadAsStringAsync().Result;
                        }
                        catch (AggregateException e)
                        {
                            if (e.InnerException.Message.Contains("ssl"))
                            {
                                PlaceDiceBet(BetDetails);
                                return;
                            }
                        }
                    }
                    if (responseData.Contains("error"))
                    {
                        if (retrycount++ < 3)
                        {

                            Thread.Sleep(200);
                            PlaceBet(BetDetails);
                            return;
                        }
                        else
                            throw new Exception();
                    }
                    string Hash = next = json.JsonDeserialize<d999Hash>(responseData).Hash;
                }
                string ClientSeed = R.Next(0, int.MaxValue).ToString();
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "PlaceBet"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("PayIn", ((long)((decimal)amount * 100000000m)).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("Low", (High ? 999999 - (int)chance : 0).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("High", (High ? 999999 : (int)chance).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("ClientSeed", ClientSeed));
                pairs.Add(new KeyValuePair<string, string>("Currency", CurrentCurrency));
                pairs.Add(new KeyValuePair<string, string>("ProtocolVersion", "2"));

                Content = new FormUrlEncodedContent(pairs);
                string tmps = Content.ReadAsStringAsync().Result;

                responseData = "";
                using (var response = Client.PostAsync("", Content))
                {

                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;

                    }
                    catch (AggregateException e)
                    {
                        Logger.DumpLog(e);
                        callError(e.ToString(),true, ErrorType.Unknown);
                    }
                }
                d999Bet tmpBet = json.JsonDeserialize<d999Bet>(responseData);

                if (amount >= 21)
                {

                }
                if (tmpBet.ChanceTooHigh == 1 || tmpBet.ChanceTooLow == 1 | tmpBet.InsufficientFunds == 1 || tmpBet.MaxPayoutExceeded == 1 || tmpBet.NoPossibleProfit == 1)
                {
                    if (tmpBet.ChanceTooHigh == 1)
                        err = "Chance too high";
                    if (tmpBet.ChanceTooLow == 1)
                        err += "Chance too Low";
                    if (tmpBet.InsufficientFunds == 1)
                        err += "Insufficient Funds";
                    if (tmpBet.MaxPayoutExceeded == 1)
                        err += "Max Payout Exceeded";
                    if (tmpBet.NoPossibleProfit == 1)
                        err += "No Possible Profit";
                    throw new Exception();
                }
                else if (tmpBet.BetId == 0)
                {
                    throw new Exception();
                }
                else
                {
                    Stats.Balance = (decimal)tmpBet.StartingBalance / 100000000.0m - (amount) + ((decimal)tmpBet.PayOut / 100000000.0m);

                    Stats.Profit += -(amount) + (decimal)(tmpBet.PayOut / 100000000m);
                    DiceBet tmp = new DiceBet();
                    tmp.Guid = BetDetails.GUID;
                    tmp.TotalAmount = (decimal)amount;
                    tmp.DateValue = DateTime.Now;
                    tmp.Chance = ((decimal)chance * 100m) / 999999m;
                    tmp.ClientSeed = ClientSeed;
                    tmp.Currency = CurrentCurrency;
                    tmp.High = High;
                    tmp.BetID = tmpBet.BetId.ToString();
                    tmp.Nonce = 0;
                    tmp.Profit = ((decimal)tmpBet.PayOut / 100000000m) - ((decimal)amount);
                    tmp.Roll = tmpBet.Secret / 10000m;
                    tmp.ServerHash = next;
                    tmp.ServerSeed = tmpBet.ServerSeed;
                    /*tmp.Userid = (int)uid;
                    tmp. = "";*/

                    bool win = false;
                    if ((tmp.Roll > 99.99m - tmp.Chance && High) || (tmp.Roll < tmp.Chance && !High))
                    {
                        win = true;
                    }
                    if (win)
                        Stats.Wins++;
                    else
                        Stats.Losses++;
                    Stats.Wagered += tmp.TotalAmount;
                    Stats.Bets++;


                   // sqlite_helper.InsertSeed(tmp.serverhash, tmp.serverseed);
                    next = tmpBet.Next;
                    retrycount = 0;
                    callBetFinished(tmp);
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), -1);
                callError(e.ToString(), true, ErrorType.Unknown);
            }
        }
        DateTime Lastbalance = DateTime.Now;
        protected override void _UpdateStats()
        {
            if (sessionCookie != "" && sessionCookie != null && (DateTime.Now - Lastbalance).TotalSeconds > 60)
            {
                Lastbalance = DateTime.Now;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "GetBalance"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Currency", CurrentCurrency));
                pairs.Add(new KeyValuePair<string, string>("Stats", "1"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            _UpdateStats();
                            return ;
                        }
                    }
                }
                try
                {
                    d999Login tmplogin = json.JsonDeserialize<d999Login>(responseData);
                    Stats.Balance = (decimal)tmplogin.Balance / 100000000.0m;
                    Stats.Wagered = -(decimal)tmplogin.TotalPayIn / 100000000.0m;
                    Stats.Profit = tmplogin.TotalProfit / 100000000.0m; ;
                    Stats.Bets = (int)tmplogin.TotalBets;
                    Stats.Wins = (int)tmplogin.TotalWins;
                    Stats.Losses = (int)tmplogin.TotalLoseCount;
                }
                catch
                {

                }
            }
        }
        public class d999Register
        {
            public string AccountCookie { get; set; }
            public string SessionCookie { get; set; }
            public long Accountid { get; set; }
            public int MaxBetBatchSize { get; set; }
            public string ClientSeed { get; set; }
            public string DepositAddress { get; set; }
        }

        public class d999Login : d999Register
        {
            public decimal Balance { get; set; }
            public string Email { get; set; }
            public string EmergenctAddress { get; set; }
            public long BetCount { get; set; }
            public long BetWinCount { get; set; }
            public long BetLoseCount { get { return BetCount - BetWinCount; } }
            public decimal BetPayIn { get; set; }
            public decimal BetPayOut { get; set; }
            public decimal Profit { get { return BetPayIn + BetPayOut; } }
            public decimal Wagered { get { return BetPayOut - BetPayIn; } }

            public decimal TotalPayIn { get; set; }
            public decimal TotalPayOut { get; set; }
            public decimal TotalProfit { get { return TotalPayIn + TotalPayOut; } }

            public long TotalBets { get; set; }
            public long TotalWins { get; set; }
            public long TotalLoseCount { get { return TotalBets - TotalWins; } }
        }

        public class d999Hash
        {
            public string Hash { get; set; }
        }
        public class d999deposit
        {
            public string Address { get; set; }
        }
        public class d999Bet
        {
            public long BetId { get; set; }
            public decimal PayOut { get; set; }
            public decimal Secret { get; set; }
            public decimal StartingBalance { get; set; }
            public string ServerSeed { get; set; }
            public string Next { get; set; }

            public int ChanceTooHigh { get; set; }
            public int ChanceTooLow { get; set; }
            public int InsufficientFunds { get; set; }
            public int NoPossibleProfit { get; set; }
            public int MaxPayoutExceeded { get; set; }
        }



    }
}
