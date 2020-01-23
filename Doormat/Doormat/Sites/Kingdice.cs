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
   public class Kingdice : BaseSite, iDice
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool iskd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public string LastHash { get; set; }

        public Kingdice()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            this.MaxRoll = 99.9999m;
            this.SiteAbbreviation = "KD";
            this.SiteName = "KingDice";
            this.SiteURL = "https://kingdice.com/#/welcome?aff=221";
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
            Currencies = new string[] { "Btc" };
            SupportedGames = new Games.Games[] { Games.Games.Dice};
            this.Currency = 0;
            this.DiceBetURL = "https://kingdice.com/#/welcome?aff=221/";
            this.Edge = 0.1m;
        }

        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {

                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 15 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        UpdateStats();
                    }
                }
                catch { }
                Thread.Sleep(1000);
            }
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            iskd = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            string Username = "";
            string Password = "";
            string otp = "";
            foreach (LoginParamValue x in LoginParams)
            {
                if (x.Param.Name.ToLower() == "username")
                    Username = x.Value;
                if (x.Param.Name.ToLower() == "password")
                    Password = x.Value;
                if (x.Param.Name.ToLower() == "2fa code")
                    otp = x.Value;
            }
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip};
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://kingdice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
            Client.DefaultRequestHeaders.Add("Host", "kingdice.com");
            Client.DefaultRequestHeaders.Add("Origin", "https://kingdice.com");
            Client.DefaultRequestHeaders.Add("Referer", "https://kingdice.com");

            try
            {
                this.username = Username;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("sdb", "8043d46408307f3ac9d14931ba27c9015349bf21b7b7"));
                pairs.Add(new KeyValuePair<string, string>("2facode", otp/*==""?"undefined":twofa*/));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("login.php", Content).Result.Content.ReadAsStringAsync().Result;

                KDLogin tmplogin = json.JsonDeserialize<KDLogin>(sEmitResponse);
                if (tmplogin.code == "SUCCESS")
                {
                    accesstoken = tmplogin.token;
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                    Content = new FormUrlEncodedContent(pairs);
                    sEmitResponse = Client.PostAsync("logged.php", Content).Result.Content.ReadAsStringAsync().Result;
                    //sEmitResponse2 = Client.GetStringAsync("logged.php").Result;
                    KDLoggedIn tmpStats = json.JsonDeserialize<KDLoggedIn>(sEmitResponse);
                    if (tmpStats.code == "SUCCESS")
                    {
                        Stats.Balance = tmpStats.balance / 100000000m;
                        
                        pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                        Content = new FormUrlEncodedContent(pairs);
                        sEmitResponse = Client.PostAsync("nextroll.php", Content).Result.Content.ReadAsStringAsync().Result;
                        KDNextRoll tmphash = json.JsonDeserialize<KDNextRoll>(sEmitResponse);
                        if (tmphash.code == "SUCCESS")
                        {
                            LastHash = tmphash.round_hash;
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("username", username));
                            pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("stats/profile.php", Content).Result.Content.ReadAsStringAsync().Result;
                            KDStat tmpstats = json.JsonDeserialize<KDStat>(sEmitResponse);
                            Stats.Wagered = tmpstats.wagered / 100000000m;
                            Stats.Profit = tmpstats.profit / 100000000m;
                            Stats.Bets = (int)tmpstats.rolls;                            
                            iskd = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            
                            callLoginFinished(true);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), 1);
            }
            callLoginFinished(false);
        }

        protected override void _UpdateStats()
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("balance.php", Content).Result.Content.ReadAsStringAsync().Result;
                KDBalance tmpbal = json.JsonDeserialize<KDBalance>(sEmitResponse);
                if (tmpbal.code == "SUCCESS")
                {
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("username", username));
                    pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                    Content = new FormUrlEncodedContent(pairs);
                    sEmitResponse = Client.PostAsync("stats/profile.php", Content).Result.Content.ReadAsStringAsync().Result;
                    KDStat tmpstats = json.JsonDeserialize<KDStat>(sEmitResponse);
                    Stats.Wagered = tmpstats.wagered / 100000000m;
                    Stats.Profit = tmpstats.profit / 100000000m;
                    Stats.Bets = (int)tmpstats.rolls;                    
                    Stats.Balance = tmpbal.balance / 100000000m;                    
                }

            }
            catch(Exception e)
            {
                Logger.DumpLog(e);
            }
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                string ClientSeed = R.Next(0, 100).ToString();
                
                decimal amount = BetDetails.Amount;
                decimal chance = BetDetails.Chance;
                bool High = BetDetails.High;
                decimal tmpchance = High ? 99m - chance : chance;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("rollAmount", (amount * 100000000m).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("rollUnder", tmpchance.ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("mode", High ? "2" : "1"));
                pairs.Add(new KeyValuePair<string, string>("rollClient", ClientSeed));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));


                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("play.php", Content).Result.Content.ReadAsStringAsync().Result;
                //Lastbet = DateTime.Now;
                try
                {
                    KDBet tmp = json.JsonDeserialize<KDBet>(sEmitResponse);
                    if (tmp.roll_id != null && tmp.roll_id != null)
                    {
                        DiceBet tmpBet = new DiceBet
                        {
                            Guid = BetDetails.GUID,
                            TotalAmount = amount,
                            DateValue = DateTime.Now,
                            BetID = tmp.roll_id.ToString(),
                            Profit = tmp.roll_profit / 100000000m,
                            Roll = tmp.roll_number,
                            High = High,
                            Chance = tmp.probability,
                            Nonce = (long)tmp.provablef_serverRoll,
                            ServerHash = LastHash,
                            ServerSeed = tmp.provablef_Hash,
                            ClientSeed = ClientSeed
                        };
                        if (tmp.roll_result == "win")
                            Stats.Wins++;
                        else
                            Stats.Losses++;
                        Stats.Wagered += amount;
                        Stats.Profit += tmp.roll_profit / 100000000m;
                        Stats.Bets++;
                        LastHash = tmp.roll_next.hash;
                        Stats.Balance = tmp.balance / 100000000m;
                        callBetFinished(tmpBet);
                    }
                    else
                    {
                        callError("An unknown error has occurred. Bet will retry in 30 seconds.",true, ErrorType.Unknown);
                    }
                    //retrycount = 0;
                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                    callError(sEmitResponse,true, ErrorType.Unknown);

                }
            }
            catch (Exception e)
            { Logger.DumpLog(e); }
        }

        public class KDLogin
        {
            public string msg { get; set; }
            public string code { get; set; }
            public string token { get; set; }
        }
        public class KDLoggedIn
        {
            public decimal balance { get; set; }
            public decimal unconfirmedbalance { get; set; }
            public string username { get; set; }
            public string address { get; set; }
            public string cvalue { get; set; }
            public string code { get; set; }
            public decimal maxProfit { get; set; }
        }
        public class KDBalance
        {
            public decimal balance { get; set; }
            public decimal unconfirmedbalance { get; set; }
            public decimal maxprofit { get; set; }
            public string code { get; set; }
        }
        public class KDNextRoll
        {
            public string round_hash { get; set; }
            public long round_id { get; set; }
            public string code { get; set; }
        }
        public class KDBetNextRoll
        {
            public string hash { get; set; }
            public long id { get; set; }
        }
        public class KDBet
        {
            public decimal balance { get; set; }
            public decimal roll_number { get; set; }
            public decimal roll_id { get; set; }
            public decimal roll_under { get; set; }
            public KDBetNextRoll roll_next { get; set; }
            public string roll_time { get; set; }
            public string roll_result { get; set; }
            public decimal roll_bet { get; set; }
            public decimal roll_payout { get; set; }
            public decimal roll_profit { get; set; }
            public decimal roll_mode { get; set; }
            public decimal probability { get; set; }
            public string provablef_Hash { get; set; }
            public string provablef_Salt { get; set; }
            public decimal provablef_clientRoll { get; set; }
            public decimal provablef_serverRoll { get; set; }
        }
        public class KDStat
        {
            public string username { get; set; }
            public string registered { get; set; }
            public string lastOnline { get; set; }
            public decimal rolls { get; set; }
            public decimal wagered { get; set; }
            public decimal profit { get; set; }
            public decimal luck { get; set; }
        }

    }
}
