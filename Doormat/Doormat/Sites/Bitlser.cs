using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoormatCore.Sites
{
    public class Bitsler : BaseSite, iDice
    {
        bool IsBitsler = false;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();

        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        public static string[] sCurrencies = new string[] { "BTC", "ETH", "LTC", "BCH", "XRP", "DOGE", "DASH", "ZEC", "ETC", "NEO", "KMD", "BTG", "LSK", "DGB", "QTUM", "STRAT", "WAVES", "BURST", "BTSLR" };
        HttpClientHandler ClientHandlr;

        public Bitsler()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("API Key", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            Currencies = sCurrencies;
            DiceBetURL = "https://www.bitsler.com/?ref=seuntjie/";
            SiteURL = "https://www.bitsler.com/?ref=seuntjie";
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "BS";
            this.SiteName = "Bitsler";
            this.SiteURL = "https://bitvest.io?r=46534";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = false;
            this.CanChangeSeed = true;
            this.CanChat = false;
            this.CanGetSeed = false;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = false;
            this.CanVerify = false;

            SupportedGames = new Games.Games[] { Games.Games.Dice, Games.Games.Roulette };
            this.Currency = 0;
            this.DiceBetURL = "https://bitvest.io/bet/{0}";
            this.Edge = 1;
        }


        void GetBalanceThread()
        {
            while (IsBitsler)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats)
                {
                    lastupdate = DateTime.Now;
                    UpdateStats();
                }
                Thread.Sleep(1000);
            }
        }


        public void PlaceDiceBet(PlaceDiceBet BetObj)
        {
            try
            {



                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                /*access_token
type:dice
amount:0.00000001
condition:< or >
game:49.5
devise:btc*/
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("type", "dice"));
                pairs.Add(new KeyValuePair<string, string>("amount", BetObj.Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("condition", BetObj.High ? ">" : "<"));
                pairs.Add(new KeyValuePair<string, string>("game", !BetObj.High ? BetObj.Chance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) : (MaxRoll - BetObj.Chance).ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("devise", CurrentCurrency));
                pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpmsg = Client.PostAsync("api/bet", Content).Result;
                string sEmitResponse = tmpmsg.Content.ReadAsStringAsync().Result;
                bsBetBase bsbase = null;
                try
                {
                    bsbase = json.JsonDeserialize<bsBetBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                }
                catch (Exception e)
                {

                }

                if (bsbase != null)
                    if (bsbase._return != null)
                        if (bsbase._return.success == "true")
                        {
                            Stats.Balance = decimal.Parse(bsbase._return.new_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                            lastupdate = DateTime.Now;
                            DiceBet tmp = bsbase._return.ToBet();
                            tmp.Guid = BetObj.GUID;
                            Stats.Profit += (decimal)tmp.Profit;
                            Stats.Wagered += (decimal)tmp.TotalAmount;
                            tmp.DateValue = DateTime.Now;
                            bool win = false;
                            if ((tmp.Roll > 99.99m - tmp.Chance && tmp.High) || (tmp.Roll < tmp.Chance && !tmp.High))
                            {
                                win = true;
                            }
                            //set win
                            if (win)
                                Stats.Wins++;
                            else
                                Stats.Losses++;
                            Stats.Bets++;
                            LastBetAmount = (double)BetObj.Amount;
                            LastBet = DateTime.Now;
                            callBetFinished(tmp);
                            return;
                        }
                        else
                        {
                            if (bsbase._return.value != null)
                            {
                                if (bsbase._return.value.Contains("Bet in progress, please wait few seconds and retry."))
                                {
                                    callNotify("Bet in progress. You need to log in with your browser and place a bet manually to fix this.");
                                }
                                else
                                {
                                    callNotify(bsbase._return.value);
                                }
                            }
                        }
                //

            }
            catch (AggregateException e)
            {
                callNotify("An Unknown error has ocurred.");
            }
            catch (Exception e)
            {
                callNotify("An Unknown error has ocurred.");
            }
        }


        
        DateTime LastReset = new DateTime();
        protected override void _ResetSeed()
        {
            Thread.Sleep(100);
            try
            {
                if ((DateTime.Now - LastReset).TotalMinutes >= 3)
                {
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    pairs.Add(new KeyValuePair<string, string>("username", username));
                    pairs.Add(new KeyValuePair<string, string>("seed_client", R.Next(0, int.MaxValue).ToString()));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("api/change-seeds", Content).Result.Content.ReadAsStringAsync().Result;
                    bsResetSeedBase bsbase = json.JsonDeserialize<bsResetSeedBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    //sqlite_helper.InsertSeed(bsbase._return.last_seeds_revealed.seed_server_hashed, bsbase._return.last_seeds_revealed.seed_server_revealed);

                    //sqlite_helper.InsertSeed(bsbase._return.last_seeds_revealed.seed_server, bsbase._return.last_seeds_revealed.seed_server_revealed);
                }
                else
                {
                    callNotify("Too soon to update seed.");
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    callNotify(sEmitResponse);
                    if (e.Message.Contains("429"))
                    {
                        Thread.Sleep(2000);
                        _ResetSeed();
                    }
                }
            }
            catch
            {
                callNotify("Too soon to update seed.");
            }
            Thread.Sleep(51);
        }


        protected override void _Login(LoginParamValue[] LoginParams)
        {
            string Username = "";
            string Password = "";
            string APIKey = "";
            string twofa = "";
            foreach (LoginParamValue x in LoginParams)
            {
                if (x.Param.Name.ToLower() == "username")
                    Username = x.Value;
                if (x.Param.Name.ToLower() == "password")
                    Password = x.Value;
                if (x.Param.Name.ToLower() == "2fa code")
                    twofa = x.Value;
                if (x.Param.Name.ToLower() == "api key")
                    APIKey = x.Value;
            }
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitsler.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "DiceBot");

            try
            {

                HttpResponseMessage resp = Client.GetAsync("https://www.bitsler.com").Result;
                string s1 = "";

                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable || resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                        //cflevel = 0;
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            callNotify(SiteName + " has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        /*if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "www.bitsler.com"))
                        {
                            return;
                        }*/
                    }
                    else
                    {

                    }
                }
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                if (!string.IsNullOrWhiteSpace(twofa))
                {
                    pairs.Add(new KeyValuePair<string, string>("twofactor", twofa));
                }
                pairs.Add(new KeyValuePair<string, string>("api_key", APIKey));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpresp = Client.PostAsync("api/login", Content).Result;

                byte[] bytes = tmpresp.Content.ReadAsByteArrayAsync().Result;
                string sEmitResponse = tmpresp.Content.ReadAsStringAsync().Result;

                //getuserstats 
                bsloginbase bsbase = json.JsonDeserialize<bsloginbase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));

                if (bsbase != null)
                    if (bsbase._return != null)
                        if (bsbase._return.success == "true")
                        {
                            accesstoken = bsbase._return.access_token;
                            IsBitsler = true;
                            lastupdate = DateTime.Now;


                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("api/getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                            bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                            if (bsstatsbase != null)
                                if (bsstatsbase._return != null)
                                    if (bsstatsbase._return.success == "true")
                                    {
                                        GetStatsFromStatsBase(bsstatsbase);
                                        this.username = Username;
                                    }
                                    else
                                    {
                                        if (bsstatsbase._return.value != null)
                                        {
                                            callNotify(bsstatsbase._return.value);
                                        }
                                    }

                            IsBitsler = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            callLoginFinished(true);
                            return;
                        }
                        else
                        {
                            if (bsbase._return.value != null)
                                callNotify(bsbase._return.value);
                        }

            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
            }
            callLoginFinished(false);
        }

        DateTime LastBet = DateTime.Now;
        double LastBetAmount = 0;

        public override int _TimeToBet(PlaceBet BetDetails)
        {
            //return true;
            decimal amount = BetDetails.TotalAmount;
            int type_delay = 0;

            if (Currencies[Currency].ToLower() == "btc") {
                if (LastBetAmount <= 0.00000005 || (double)amount <= 0.00000005)
                    type_delay = 1;
                else
                    type_delay = 2;
            }
            else if (Currencies[Currency].ToLower() == "eth")
            {
                if (LastBetAmount <= 0.00000250 || (double)amount <= 0.00000250)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "ltc")
            {
                if (LastBetAmount <= 0.00001000 || (double)amount <= 0.00001000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "doge")
            {
                if (LastBetAmount <= 5.00000000 || (double)amount <= 5.00000000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "burst")
            {
                if (LastBetAmount <= 5.00000000 || (double)amount <= 5.00000000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "bch")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "dash")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "zec")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "xmr")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "etc")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "neo")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "strat")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "kmd")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currencies[Currency].ToLower() == "xrp")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            int delay = 0;
            if (type_delay == 1)
                delay = 300;
            else if (type_delay == 2)
                delay = 200;
            else
                delay = 200;

            return delay - (int)((DateTime.Now - LastBet).TotalMilliseconds);
        }



        public static decimal sGetLucky(string Hash, string server, string client, int nonce)
        {
            SHA1 betgenerator = SHA1.Create();
            string Seed = server + "-" + client + "-" + nonce;
            byte[] serverb = new byte[Seed.Length];

            for (int i = 0; i < Seed.Length; i++)
            {
                serverb[i] = Convert.ToByte(Seed[i]);
            }
            decimal Lucky = 0;
            do
            {
                serverb = betgenerator.ComputeHash(serverb.ToArray());
                StringBuilder hex = new StringBuilder(serverb.Length * 2);
                foreach (byte b in serverb)
                    hex.AppendFormat("{0:x2}", b);

                string s = hex.ToString().Substring(0, 8);
                Lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            } while (Lucky > 4294960000);
            Lucky = (Lucky % 10000.0m) / 100.0m;
            if (Lucky < 0)
                return -Lucky;
            return Lucky;
        }

        protected override decimal _GetLucky(string Hash, string server, string client, int nonce)
        {

            return sGetLucky(Hash, server, client, nonce);
        }


        protected override void _Disconnect()
        {
            this.IsBitsler = false;
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        void GetStatsFromStatsBase(bsStatsBase bsstatsbase)
        {
            switch (Currencies[Currency].ToLower())
            {
                case "btc":
                    Stats.Balance = bsstatsbase._return.btc_balance;
                    Stats.Profit = bsstatsbase._return.btc_profit;
                    Stats.Wagered = bsstatsbase._return.btc_wagered; break;
                case "ltc":
                    Stats.Balance = bsstatsbase._return.ltc_balance;
                    Stats.Profit = bsstatsbase._return.ltc_profit;
                    Stats.Wagered = bsstatsbase._return.ltc_wagered; break;
                case "doge":
                    Stats.Balance = bsstatsbase._return.doge_balance;
                    Stats.Profit = bsstatsbase._return.doge_profit;
                    Stats.Wagered = bsstatsbase._return.doge_wagered; break;
                case "eth":
                    Stats.Balance = bsstatsbase._return.eth_balance;
                    Stats.Profit = bsstatsbase._return.eth_profit;
                    Stats.Wagered = bsstatsbase._return.eth_wagered; break;
                case "burst":
                    Stats.Balance = bsstatsbase._return.burst_balance;
                    Stats.Profit = bsstatsbase._return.burst_profit;
                    Stats.Wagered = bsstatsbase._return.burst_wagered; break;
                case "dash":
                    Stats.Balance = bsstatsbase._return.dash_balance;
                    Stats.Profit = bsstatsbase._return.dash_profit;
                    Stats.Wagered = bsstatsbase._return.dash_wagered; break;
                case "zec":
                    Stats.Balance = bsstatsbase._return.zec_balance;
                    Stats.Profit = bsstatsbase._return.zec_profit;
                    Stats.Wagered = bsstatsbase._return.zec_wagered; break;
                case "bch":
                    Stats.Balance = bsstatsbase._return.bch_balance;
                    Stats.Profit = bsstatsbase._return.bch_profit;
                    Stats.Wagered = bsstatsbase._return.bch_wagered; break;
                case "xmr":
                    Stats.Balance = bsstatsbase._return.xmr_balance;
                    Stats.Profit = bsstatsbase._return.xmr_profit;
                    Stats.Wagered = bsstatsbase._return.xmr_wagered; break;
                case "etc":
                    Stats.Balance = bsstatsbase._return.etc_balance;
                    Stats.Profit = bsstatsbase._return.etc_profit;
                    Stats.Wagered = bsstatsbase._return.etc_wagered; break;
                case "neo":
                    Stats.Balance = bsstatsbase._return.neo_balance;
                    Stats.Profit = bsstatsbase._return.neo_profit;
                    Stats.Wagered = bsstatsbase._return.neo_wagered; break;
                case "strat":
                    Stats.Balance = bsstatsbase._return.strat_balance;
                    Stats.Profit = bsstatsbase._return.strat_profit;
                    Stats.Wagered = bsstatsbase._return.strat_wagered; break;
                case "kmd":
                    Stats.Balance = bsstatsbase._return.kmd_balance;
                    Stats.Profit = bsstatsbase._return.kmd_profit;
                    Stats.Wagered = bsstatsbase._return.kmd_wagered; break;
                case "xrp":
                    Stats.Balance = bsstatsbase._return.xrp_balance;
                    Stats.Profit = bsstatsbase._return.xrp_profit;
                    Stats.Wagered = bsstatsbase._return.xrp_wagered; break;
                case "btg":
                    Stats.Balance = bsstatsbase._return.btg_balance;
                    Stats.Profit = bsstatsbase._return.btg_profit;
                    Stats.Wagered = bsstatsbase._return.btg_wagered; break;
                case "qtum":
                    Stats.Balance = bsstatsbase._return.qtum_balance;
                    Stats.Profit = bsstatsbase._return.qtum_profit;
                    Stats.Wagered = bsstatsbase._return.qtum_wagered; break;
                case "lsk":
                    Stats.Balance = bsstatsbase._return.lsk_balance;
                    Stats.Profit = bsstatsbase._return.lsk_profit;
                    Stats.Wagered = bsstatsbase._return.lsk_wagered; break;
                case "dgb":
                    Stats.Balance = bsstatsbase._return.dgb_balance;
                    Stats.Profit = bsstatsbase._return.dgb_profit;
                    Stats.Wagered = bsstatsbase._return.dgb_wagered; break;
                case "waves":
                    Stats.Balance = bsstatsbase._return.waves_balance;
                    Stats.Profit = bsstatsbase._return.waves_profit;
                    Stats.Wagered = bsstatsbase._return.waves_wagered; break;
                case "btslr":
                    Stats.Balance = bsstatsbase._return.btslr_balance;
                    Stats.Profit = bsstatsbase._return.btslr_profit;
                    Stats.Wagered = bsstatsbase._return.btslr_wagered; break;
            }
            Stats.Bets = int.Parse(bsstatsbase._return.bets, System.Globalization.NumberFormatInfo.InvariantInfo);
            Stats.Wins = int.Parse(bsstatsbase._return.wins, System.Globalization.NumberFormatInfo.InvariantInfo);
            Stats.Losses = int.Parse(bsstatsbase._return.losses, System.Globalization.NumberFormatInfo.InvariantInfo);

        }

        protected override void _UpdateStats()
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage resp = Client.PostAsync("api/getuserstats", Content).Result;

                string s1 = "";
                string sEmitResponse = "";// resp.Content.ReadAsStringAsync().Result;

                if (resp.IsSuccessStatusCode)
                {
                    sEmitResponse = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    sEmitResponse = "";
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {

                    }
                }
                if (sEmitResponse != "")
                {
                    bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    if (bsstatsbase != null)
                        if (bsstatsbase._return != null)
                            if (bsstatsbase._return.success == "true")
                            {
                                GetStatsFromStatsBase(bsstatsbase);
                            }
                            else
                            {
                                if (bsstatsbase._return.value != null)
                                {
                                    callNotify(bsstatsbase._return.value);
                                }
                            }
                }
            }
            catch { }
        }

        public class bsLogin
    {
        public string success { get; set; }
        public string value { get; set; }
        public string access_token { get; set; }
    }
    public class bsloginbase
    {
        public bsLogin _return { get; set; }
    }
    //"{\"return\":{\"success\":\"true\",\"balance\":1.0e-5,\"wagered\":0,\"profit\":0,\"bets\":\"0\",\"wins\":\"0\",\"losses\":\"0\"}}"
    //{"return":{"success":"true","bets":0,"wins":0,"losses":0,"btc_profit":"0.00000000","btc_wagered":"0.00000000","btc_balance":"0.00000000","eth_profit":"0.00000000","eth_wagered":"0.00000000","eth_balance":"0.00000000","ltc_profit":"0.00000000","ltc_wagered":"0.00000000","ltc_balance":"0.00000000","bch_profit":"0.00000000","bch_wagered":"0.00000000","bch_balance":"0.00000000","doge_profit":"0.00000000","doge_wagered":"0.00000000","doge_balance":"0.00000000","dash_profit":"0.00000000","dash_wagered":"0.00000000","dash_balance":"0.00000000","zec_profit":"0.00000000","zec_wagered":"0.00000000","zec_balance":"0.00000000","burst_profit":"0.00000000","burst_wagered":"0.00000000","burst_balance":"0.00000000"}}
    public class bsStats
    {
        public string success { get; set; }
        public string value { get; set; }
        public decimal btc_balance { get; set; }
        public decimal btc_wagered { get; set; }
        public decimal btc_profit { get; set; }
        public string bets { get; set; }
        public decimal ltc_balance { get; set; }
        public decimal ltc_wagered { get; set; }
        public decimal ltc_profit { get; set; }

        public decimal doge_balance { get; set; }
        public decimal doge_wagered { get; set; }
        public decimal doge_profit { get; set; }
        public decimal eth_balance { get; set; }
        public decimal eth_wagered { get; set; }
        public decimal eth_profit { get; set; }
        public decimal burst_balance { get; set; }
        public decimal burst_wagered { get; set; }
        public decimal zec_profit { get; set; }
        public decimal zec_balance { get; set; }
        public decimal zec_wagered { get; set; }
        public decimal bch_profit { get; set; }
        public decimal bch_balance { get; set; }
        public decimal bch_wagered { get; set; }
        public decimal dash_profit { get; set; }
        public decimal dash_balance { get; set; }
        public decimal dash_wagered { get; set; }
        public decimal burst_profit { get; set; }
        public decimal etc_balance { get; set; }
        public decimal etc_wagered { get; set; }
        public decimal etc_profit { get; set; }

        public decimal xmr_balance { get; set; }
        public decimal xmr_wagered { get; set; }
        public decimal xmr_profit { get; set; }

        public decimal neo_balance { get; set; }
        public decimal neo_wagered { get; set; }
        public decimal neo_profit { get; set; }

        public decimal strat_balance { get; set; }
        public decimal strat_wagered { get; set; }
        public decimal strat_profit { get; set; }

        public decimal kmd_balance { get; set; }
        public decimal kmd_wagered { get; set; }
        public decimal kmd_profit { get; set; }

        public decimal xrp_balance { get; set; }
        public decimal xrp_wagered { get; set; }
        public decimal xrp_profit { get; set; }

        public decimal btg_balance { get; set; }
        public decimal btg_wagered { get; set; }
        public decimal btg_profit { get; set; }

        public decimal lsk_balance { get; set; }
        public decimal lsk_wagered { get; set; }
        public decimal lsk_profit { get; set; }

        public decimal dgb_balance { get; set; }
        public decimal dgb_wagered { get; set; }
        public decimal dgb_profit { get; set; }

        public decimal qtum_balance { get; set; }
        public decimal qtum_wagered { get; set; }
        public decimal qtum_profit { get; set; }

        public decimal waves_balance { get; set; }
        public decimal waves_wagered { get; set; }
        public decimal waves_profit { get; set; }

            public decimal btslr_balance { get; set; }
            public decimal btslr_wagered { get; set; }
            public decimal btslr_profit { get; set; }

            public string wins { get; set; }
        public string losses { get; set; }
    }
    public class bsStatsBase
    {
        public bsStats _return { get; set; }
    }
    public class bsBetBase
    {
        public bsBet _return { get; set; }
    }
    public class bsBet
    {
        public string success { get; set; }
        public string value { get; set; }
        public string username { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string devise { get; set; }
        public long ts { get; set; }
        public string time { get; set; }
        public string amount { get; set; }
        public decimal roll_number { get; set; }
        public string condition { get; set; }
        public string game { get; set; }
        public decimal payout { get; set; }
        public string winning_chance { get; set; }
        public string amount_return { get; set; }
        public string new_balance { get; set; }
        public string _event { get; set; }
        public string server_seed { get; set; }
        public string client_seed { get; set; }
        public long nonce { get; set; }

        public DiceBet ToBet()
        {
            DiceBet tmp = new DiceBet
            {
                TotalAmount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                DateValue = json.ToDateTime2(ts.ToString()),
                BetID = id,
                Profit = decimal.Parse(amount_return, System.Globalization.NumberFormatInfo.InvariantInfo),
                Roll = (decimal)roll_number,
                High = condition == ">",
                Chance = decimal.Parse(winning_chance, System.Globalization.NumberFormatInfo.InvariantInfo),
                Nonce = nonce,
                ServerHash= server_seed,
                ClientSeed = client_seed
            };
            return tmp;
        }
    }
    public class bsResetSeedBase
    {
        public bsResetSeed _return { get; set; }
    }
    public class bsResetSeed
    {
        public string seed_server_hashed { get; set; }
        public string seed_server { get; set; }
        public string seed_client { get; set; }
        public string nonce { get; set; }
        public string seed_server_revealed { get; set; }
        public bsResetSeed last_seeds_revealed { get; set; }
    }
}
}
