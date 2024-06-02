using Gambler.Bot.Common.Enums;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites.Classes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Sites
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

        public Bitsler(ILogger logger):base(logger)
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
            NonceBased = true;
            SupportedGames = new Games[] { Games.Dice };
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


        public async Task<DiceBet> PlaceDiceBet(PlaceDiceBet BetObj)
        {
            try
            {
                if (BetObj.Chance>99.99m)
                {
                    callError("Chance must be less than 99.99", false, ErrorType.InvalidBet);
                    return null;
                }


                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                /*access_token
type:dice
amount:0.00000001
condition:< or >
game:49.5
devise:btc*/
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                //pairs.Add(new KeyValuePair<string, string>("type", "dice"));
                pairs.Add(new KeyValuePair<string, string>("amount", BetObj.Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("over", BetObj.High.ToString().ToLower()));
                pairs.Add(new KeyValuePair<string, string>("target", !BetObj.High ? BetObj.Chance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) : (MaxRoll - BetObj.Chance).ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("currency", CurrentCurrency));
                pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpmsg = await Client.PostAsync("api/bet-dice", Content);
                string sEmitResponse =await tmpmsg.Content.ReadAsStringAsync();
                bsBet bsbase = null;
                try
                {
                    bsbase = JsonSerializer.Deserialize<bsBet>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                }
                catch (Exception e)
                {

                }

                if (bsbase != null)
                    // if (bsbase._return != null)
                    if (bsbase.success)
                    {
                        Stats.Balance = decimal.Parse(bsbase.new_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                        lastupdate = DateTime.Now;
                        DiceBet tmp = bsbase.ToBet();
                        tmp.High = BetObj.High;
                        tmp.Chance = BetObj.Chance;
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
                        return tmp;
                    }
                    else
                    {
                        if (bsbase.error != null)
                        {
                             ErrorType type = ErrorType.Unknown;
                            
                            if (bsbase.error.StartsWith("Maximum bet") )
                            {
                                type = ErrorType.InvalidBet;
                            }
                            else if (bsbase.error== "Bet amount not valid")
                            {
                                type = ErrorType.BetTooLow;
                            }
                            else if (bsbase.error.Contains("Bet in progress, please wait few seconds and retry."))
                            {
                                
                            }
                            else if (bsbase.error == "Insufficient fund")
                                type = ErrorType.BalanceTooLow;
                            else
                            {
                                
                            }
                            callError(bsbase.error, false, type);
                            return null;
                        }
                    }
                //

            }
            catch (AggregateException e)
            {
                callError("An Unknown error has ocurred.", false, ErrorType.Unknown);
                callNotify("An Unknown error has ocurred.");
            }
            catch (Exception e)
            {
                callError("An Unknown error has ocurred.", false, ErrorType.Unknown);
                callNotify("An Unknown error has ocurred.");
            }
            return null;
        }



        DateTime LastReset = new DateTime();
        protected override async Task<SeedDetails> _ResetSeed()
        {
            Thread.Sleep(100);
            try
            {
                if ((DateTime.Now - LastReset).TotalMinutes >= 3)
                {
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    pairs.Add(new KeyValuePair<string, string>("username", username));
                    string clientseed = GenerateNewClientSeed();
                    pairs.Add(new KeyValuePair<string, string>("seed_client", clientseed));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("api/change-seeds", Content).Result.Content.ReadAsStringAsync().Result;
                    bsResetSeedBase bsbase = JsonSerializer.Deserialize<bsResetSeedBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    callResetSeedFinished(true, "");
                    return new SeedDetails
                    {
                        ClientSeed = clientseed,
                        Nonce = 0,
                        PreviousClient = bsbase._return.previous_client,
                        PreviousHash = bsbase._return.previous_hash,
                        ServerHash = bsbase._return.current_hash,
                        PreviousServer = bsbase._return.previous_seed,
                        ServerSeed = bsbase._return.next_hash
                    };
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
                        return await _ResetSeed();
                    }
                }
            }
            catch
            {
                callNotify("Too soon to update seed.");
            }
            Thread.Sleep(51);
            callResetSeedFinished(false, "");
            return null;
        }


        protected override async Task<bool> _Login(LoginParamValue[] LoginParams)
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
                    APIKey = x.Value?.Trim();
            }
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitsler.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "DiceBot");

            try
            {

                HttpResponseMessage resp = await Client.GetAsync("https://www.bitsler.com");
                string s1 = "";


                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                if (!string.IsNullOrWhiteSpace(twofa))
                {
                    pairs.Add(new KeyValuePair<string, string>("two_factor", twofa));
                }
                pairs.Add(new KeyValuePair<string, string>("api_key", APIKey));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpresp = await Client.PostAsync("api/login", Content);

                byte[] bytes = await tmpresp.Content.ReadAsByteArrayAsync();
                string sEmitResponse = await tmpresp.Content.ReadAsStringAsync();

                //getuserstats 
                bsLogin bsbase = JsonSerializer.Deserialize<bsLogin>(sEmitResponse.Replace("\"return\":", "\"_return\":"));


                if (bsbase?.success ?? false)
                {
                    accesstoken = bsbase.access_token;
                    IsBitsler = true;
                    lastupdate = DateTime.Now;


                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    Content = new FormUrlEncodedContent(pairs);
                    resp = await Client.PostAsync("api/getuserstats", Content);
                    sEmitResponse = await resp.Content.ReadAsStringAsync();
                    bsStats bsstatsbase = JsonSerializer.Deserialize<bsStats>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    if (bsstatsbase != null)

                        if (bsstatsbase.success)
                        {
                            GetStatsFromStatsBase(bsstatsbase);
                            this.username = Username;
                        }
                        else
                        {
                            if (bsstatsbase.error != null)
                            {
                                callNotify(bsstatsbase.error);
                            }
                        }

                    IsBitsler = true;
                    Thread t = new Thread(GetBalanceThread);
                    t.Start();
                    callLoginFinished(true);
                    return true;
                }
                else
                {
                    if (bsbase.error != null)
                        callNotify(bsbase.error);
                }

            }
            catch (Exception e)
            {
                _logger?.LogError(e.ToString());
            }
            callLoginFinished(false);
            return false;
        }

        DateTime LastBet = DateTime.Now;
        double LastBetAmount = 0;

        public override int _TimeToBet(PlaceBet BetDetails)
        {
            //return true;
            decimal amount = BetDetails.TotalAmount;
            int type_delay = 0;

            if (Currencies[Currency].ToLower() == "btc")
            {
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



        public static decimal sGetLucky(string server, string client, int nonce)
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

        protected override decimal _GetLucky( string server, string client, int nonce)
        {

            return sGetLucky( server, client, nonce);
        }


        protected override void _Disconnect()
        {
            this.IsBitsler = false;
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        void GetStatsFromStatsBase(bsStats bsstatsbase)
        {
            switch (Currencies[Currency].ToLower())
            {
                case "btc":
                    Stats.Balance = decimal.Parse(bsstatsbase.btc_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.btc_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.btc_wagered, NumberFormatInfo.InvariantInfo); break;
                case "ltc":
                    Stats.Balance = decimal.Parse(bsstatsbase.ltc_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.ltc_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.ltc_wagered, NumberFormatInfo.InvariantInfo); break;
                case "doge":
                    Stats.Balance = decimal.Parse(bsstatsbase.doge_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.doge_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.doge_wagered, NumberFormatInfo.InvariantInfo); break;
                case "eth":
                    Stats.Balance = decimal.Parse(bsstatsbase.eth_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.eth_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.eth_wagered, NumberFormatInfo.InvariantInfo); break;

                case "dash":
                    Stats.Balance = decimal.Parse(bsstatsbase.dash_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.dash_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.dash_wagered, NumberFormatInfo.InvariantInfo); break;
                case "zec":
                    Stats.Balance = decimal.Parse(bsstatsbase.zec_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.zec_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.zec_wagered, NumberFormatInfo.InvariantInfo); break;
                case "bch":
                    Stats.Balance = decimal.Parse(bsstatsbase.bch_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.bch_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.bch_wagered, NumberFormatInfo.InvariantInfo); break;

                case "etc":
                    Stats.Balance = decimal.Parse(bsstatsbase.etc_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.etc_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.etc_wagered, NumberFormatInfo.InvariantInfo); break;
                case "neo":
                    Stats.Balance = decimal.Parse(bsstatsbase.neo_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.neo_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.neo_wagered, NumberFormatInfo.InvariantInfo); break;

                case "xrp":
                    Stats.Balance = decimal.Parse(bsstatsbase.xrp_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.xrp_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.xrp_wagered, NumberFormatInfo.InvariantInfo); break;
                case "btg":
                    Stats.Balance = decimal.Parse(bsstatsbase.btg_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.btg_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.btg_wagered, NumberFormatInfo.InvariantInfo); break;
                case "qtum":
                    Stats.Balance = decimal.Parse(bsstatsbase.qtum_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.qtum_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.qtum_wagered, NumberFormatInfo.InvariantInfo); break;

                case "dgb":
                    Stats.Balance = decimal.Parse(bsstatsbase.dgb_balance, NumberFormatInfo.InvariantInfo);
                    Stats.Profit = decimal.Parse(bsstatsbase.dgb_profit, NumberFormatInfo.InvariantInfo);
                    Stats.Wagered = decimal.Parse(bsstatsbase.dgb_wagered, NumberFormatInfo.InvariantInfo); break;


            }
            Stats.Bets = bsstatsbase.bets;
            Stats.Wins = bsstatsbase.wins;
            Stats.Losses = bsstatsbase.losses;

        }

        protected override async Task<SiteStats> _UpdateStats()
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
                    sEmitResponse = await resp.Content.ReadAsStringAsync();
                }
                else
                {
                    sEmitResponse = "";
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = await resp.Content.ReadAsStringAsync();
                    }
                    else
                    {

                    }
                }
                if (sEmitResponse != "")
                {
                    bsStats bsstatsbase = JsonSerializer.Deserialize<bsStats>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    if (bsstatsbase != null)
                        //if (bsstatsbase._return != null)
                        if (bsstatsbase.success)
                        {
                            GetStatsFromStatsBase(bsstatsbase);
                        }
                        else
                        {
                            if (bsstatsbase.error != null)
                            {
                                callNotify(bsstatsbase.error);
                            }
                        }
                }
                return Stats;
            }
            catch (Exception e) 
            {
                _logger?.LogError(e.ToString());
                callError("Failed to update stats", false, ErrorType.Other);
                return null; 
            }
        }


        public class bsLogin
        {
            public bool success { get; set; }
            public string access_token { get; set; }
            public string session_token { get; set; }
            public string error { get; set; }
        }

        public class bsloginbase
        {
            public bsLogin _return { get; set; }
        }
        //"{\"return\":{\"success\":\"true\",\"balance\":1.0e-5,\"wagered\":0,\"profit\":0,\"bets\":\"0\",\"wins\":\"0\",\"losses\":\"0\"}}"
        //{"return":{"success":"true","bets":0,"wins":0,"losses":0,"btc_profit":"0.00000000","btc_wagered":"0.00000000","btc_balance":"0.00000000","eth_profit":"0.00000000","eth_wagered":"0.00000000","eth_balance":"0.00000000","ltc_profit":"0.00000000","ltc_wagered":"0.00000000","ltc_balance":"0.00000000","bch_profit":"0.00000000","bch_wagered":"0.00000000","bch_balance":"0.00000000","doge_profit":"0.00000000","doge_wagered":"0.00000000","doge_balance":"0.00000000","dash_profit":"0.00000000","dash_wagered":"0.00000000","dash_balance":"0.00000000","zec_profit":"0.00000000","zec_wagered":"0.00000000","zec_balance":"0.00000000","burst_profit":"0.00000000","burst_wagered":"0.00000000","burst_balance":"0.00000000"}}


        public class bsStats
        {
            public string error { get; set; }
            public bool success { get; set; }
            public int bets { get; set; }
            public int wins { get; set; }
            public int losses { get; set; }
            public string btc_profit { get; set; }
            public string btc_wagered { get; set; }
            public string eth_profit { get; set; }
            public string eth_wagered { get; set; }
            public string xrp_profit { get; set; }
            public string xrp_wagered { get; set; }
            public string ltc_profit { get; set; }
            public string ltc_wagered { get; set; }
            public string doge_profit { get; set; }
            public string doge_wagered { get; set; }
            public string etc_profit { get; set; }
            public string etc_wagered { get; set; }
            public string bnb_profit { get; set; }
            public string bnb_wagered { get; set; }
            public string busd_profit { get; set; }
            public string busd_wagered { get; set; }
            public string usdc_profit { get; set; }
            public string usdc_wagered { get; set; }
            public string sol_profit { get; set; }
            public string sol_wagered { get; set; }
            public string ada_profit { get; set; }
            public string ada_wagered { get; set; }
            public string bch_profit { get; set; }
            public string bch_wagered { get; set; }
            public string dash_profit { get; set; }
            public string dash_wagered { get; set; }
            public string btg_profit { get; set; }
            public string btg_wagered { get; set; }
            public string zec_profit { get; set; }
            public string zec_wagered { get; set; }
            public string dgb_profit { get; set; }
            public string dgb_wagered { get; set; }
            public string eos_profit { get; set; }
            public string eos_wagered { get; set; }
            public string xlm_profit { get; set; }
            public string xlm_wagered { get; set; }
            public string trx_profit { get; set; }
            public string trx_wagered { get; set; }
            public string neo_profit { get; set; }
            public string neo_wagered { get; set; }
            public string qtum_profit { get; set; }
            public string qtum_wagered { get; set; }
            public string usdt_profit { get; set; }
            public string usdt_wagered { get; set; }
            public string ethw_profit { get; set; }
            public string ethw_wagered { get; set; }
            public string matic_profit { get; set; }
            public string matic_wagered { get; set; }
            public string shib_profit { get; set; }
            public string shib_wagered { get; set; }
            public string link_profit { get; set; }
            public string link_wagered { get; set; }
            public string dai_profit { get; set; }
            public string dai_wagered { get; set; }
            public string ton_profit { get; set; }
            public string ton_wagered { get; set; }
            public string avax_profit { get; set; }
            public string avax_wagered { get; set; }
            public string btslr_profit { get; set; }
            public string btslr_wagered { get; set; }
            public string brl_profit { get; set; }
            public string brl_wagered { get; set; }
            public string fdusd_profit { get; set; }
            public string fdusd_wagered { get; set; }
            public string btc_balance { get; set; }
            public string eth_balance { get; set; }
            public string ltc_balance { get; set; }
            public string doge_balance { get; set; }
            public string bch_balance { get; set; }
            public string etc_balance { get; set; }
            public string zec_balance { get; set; }
            public string neo_balance { get; set; }
            public string dgb_balance { get; set; }
            public string btg_balance { get; set; }
            public string qtum_balance { get; set; }
            public string bnb_balance { get; set; }
            public string ethw_balance { get; set; }
            public string sol_balance { get; set; }
            public string ada_balance { get; set; }
            public string usdt_balance { get; set; }
            public string trx_balance { get; set; }
            public string matic_balance { get; set; }
            public string xlm_balance { get; set; }
            public string xrp_balance { get; set; }
            public string dash_balance { get; set; }
            public string eos_balance { get; set; }
        }



        public class bsBetBase
        {
            public bsBet _return { get; set; }
        }



        public class bsBet
        {
            public bool success { get; set; }
            public string username { get; set; }
            public string id { get; set; }
            public string currency { get; set; }
            public int timestamp { get; set; }
            public string amount { get; set; }
            public float result { get; set; }
            public decimal payout { get; set; }
            public string profit { get; set; }
            public string new_balance { get; set; }
            public float xp { get; set; }
            public float xp_add { get; set; }
            public string server_seed { get; set; }
            public string client_seed { get; set; }
            public int nonce { get; set; }
            public object[] notifications { get; set; }
            
            public string error { get; set; }
            public DiceBet ToBet()
            {
                DiceBet tmp = new DiceBet
                {
                    TotalAmount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                    DateValue = DateTime.Now,
                    BetID = id,
                    Profit = decimal.Parse(profit, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Roll = (decimal)result,
                   
                    Nonce = nonce,
                    ServerHash = server_seed,
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
            public string previous_hash { get; set; }
            public string previous_seed { get; set; }
            public string previous_client { get; set; }
            public string previous_total { get; set; }
            public string current_client { get; set; }
            public string current_hash { get; set; }
            public string next_hash { get; set; }
            public bool success { get; set; }
        }

    }
}
