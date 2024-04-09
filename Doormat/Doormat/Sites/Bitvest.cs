using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;

namespace DoormatCore.Sites
{
    public class Bitvest : BaseSite, iDice
    {
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public bool isbv = false;
        string accesstoken = "";
        DateTime lastupdate = new DateTime();
        int retrycount = 0;
        DateTime Lastbet = DateTime.Now;
        string secret = "";
        //bitvestCurWeight Weights = null;
        double[] Limits = new double[0];
        string pw = "";
        string lasthash = "";
        string[] CurrencyMap = new string[] { };
        string seed = "";

        public Bitvest()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "BV";
            this.SiteName = "Bitvest";
            this.SiteURL = "https://bitvest.io?r=46534";
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
            this.Currencies = new string[] { "btc", "tok", "ltc", "eth", "doge", "bch" };
            CurrencyMap = new string[] { "bitcoins", "tokens", "litecoins", "ethers", "dogecoins", "bcash" };
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://bitvest.io/bet/{0}";
            this.Edge = 1;
        }


        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            isbv = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
         | SecurityProtocolType.Tls11
         | SecurityProtocolType.Tls12;
            //ServicePointManager.SecurityProtocol &= SecurityProtocolType.Ssl3;
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip/*, Proxy = this.Prox, UseProxy = Prox != null*/ };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://bitvest.io/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));

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

            try
            {
                string resp = "";// Client.GetStringAsync("").Result;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("type", "secret"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                resp = Client.PostAsync("https://bitvest.io/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                bitvestLoginBase tmpblogin = JsonSerializer.Deserialize<bitvestLoginBase>(resp.Replace("-", "_"));
                bitvestLogin tmplogin = tmpblogin.data;
                secret = tmpblogin.account.secret;
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                pairs.Add(new KeyValuePair<string, string>("k", "0"));
                pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                pairs.Add(new KeyValuePair<string, string>("u", "0"));
                //pairs.Add(new KeyValuePair<string, string>("self_only", "1"));
                Content = new FormUrlEncodedContent(pairs);
                resp = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;

                string tmpresp = resp.Replace("-", "_");
                tmpblogin = JsonSerializer.Deserialize<bitvestLoginBase>(tmpresp);
                tmplogin = tmpblogin.data;
                if (tmplogin.session_token != null)
                {
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("username", Username));
                    pairs.Add(new KeyValuePair<string, string>("password", Password));
                    pairs.Add(new KeyValuePair<string, string>("tfa", otp));
                    pairs.Add(new KeyValuePair<string, string>("token", tmplogin.session_token));
                    //pairs.Add(new KeyValuePair<string, string>("c", "secret"));
                    pairs.Add(new KeyValuePair<string, string>("secret", secret));
                    Content = new FormUrlEncodedContent(pairs);
                    resp = Client.PostAsync("https://bitvest.io/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                    tmpresp = resp.Replace("-", "_");
                    tmpblogin = JsonSerializer.Deserialize<bitvestLoginBase>(tmpresp);
                    //Weights = tmpblogin.currency_weight;
                    Limits = tmpblogin.rate_limits;

                    tmplogin = tmpblogin.data;
                    if (Currencies[0].ToLower() == "btc")
                    {
                        Stats.Balance = decimal.Parse(tmplogin.balance ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Wagered = decimal.Parse(tmplogin.self_total_bet_dice??"0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Profit = decimal.Parse(tmplogin.self_total_won_dice ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);

                    }
                    else if (Currencies[0].ToLower() == "eth")
                    {
                        Stats.Balance = decimal.Parse(tmplogin.balance_ether ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Wagered = decimal.Parse(tmplogin.self_total_bet_ether ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Profit = decimal.Parse(tmplogin.self_total_won_ether ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currencies[0].ToLower() == "ltc")
                    {
                        Stats.Balance = decimal.Parse(tmplogin.balance_litecoin ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Wagered = decimal.Parse(tmplogin.self_total_bet_litecoin ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Profit = decimal.Parse(tmplogin.self_total_won_litecoin ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currencies[0].ToLower() == "bch")
                    {
                        Stats.Balance = decimal.Parse(tmplogin.balance_bcash ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Wagered = decimal.Parse(tmplogin.self_total_won_bcash ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Profit = decimal.Parse(tmplogin.self_total_won_bcash ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currencies[0].ToLower() == "doge")
                    {
                        Stats.Balance = decimal.Parse(tmplogin.balance_dogecoin ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Wagered = decimal.Parse(tmplogin.self_total_won_dogecoin ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Profit = decimal.Parse(tmplogin.self_total_won_dogecoin ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        Stats.Balance = decimal.Parse(tmplogin.token_balance ?? "0", System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    accesstoken = tmplogin.session_token;
                    secret = tmpblogin.account.secret;
                    Stats.Bets = int.Parse(tmplogin.self_total_bets_dice.Replace(",", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                    Stats.Wins = 0;
                    Stats.Losses = 0;
                    seed = tmpblogin.last_user_seed;

                    lastupdate = DateTime.Now;
                    isbv = true;
                    pw = Password;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    lasthash = tmpblogin.server_hash;
                    this.CanTip = tmpblogin.tip.enabled;
                    callLoginFinished(true);
                    return;
                }
                else
                {
                    callLoginFinished(false);
                    return;
                }

            }
            catch (AggregateException e)
            {
                Logger.DumpLog(e);
                callLoginFinished(false);
                return;
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                callLoginFinished(false);
                return;
            }
            callLoginFinished(false);
        }


        string RandomSeed()
        {
            string s = "";
            string chars = "0123456789abcdef";
            int length = R.Next(8, 64);
            while (s.Length <= length)
            {
                s += chars[R.Next(0, chars.Length)];
            }

            return s;
        }
        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(seed))
                    seed = RandomSeed();
                decimal amount = BetDetails.Amount;
                decimal chance = BetDetails.Chance;
                bool High = BetDetails.High;

                decimal tmpchance = High ? MaxRoll - chance + 0.0001m : chance - 0.0001m;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

                pairs.Add(new KeyValuePair<string, string>("bet", (amount).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("target", tmpchance.ToString("0.0000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("side", High ? "high" : "low"));
                pairs.Add(new KeyValuePair<string, string>("act", "play_dice"));
                pairs.Add(new KeyValuePair<string, string>("currency", CurrencyMap[Currency]));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("user_seed", seed));
                pairs.Add(new KeyValuePair<string, string>("v", "65535"));


                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                Lastbet = DateTime.Now;
                try
                {
                    string x = sEmitResponse.Replace("f-", "f_").Replace("n-", "n_").Replace("ce-", "ce_").Replace("r-", "r_");
                    bitvestbet tmp = JsonSerializer.Deserialize<bitvestbet>(x);
                    if (tmp.success)
                    {
                        DiceBet resbet = new DiceBet
                        {
                            TotalAmount = amount,
                            DateValue = DateTime.Now,
                            Chance = chance,
                            High = High,
                            ClientSeed = seed,
                            ServerHash = tmp.server_hash,
                            ServerSeed = tmp.server_seed,
                            Roll = (decimal)tmp.game_result.roll,
                            Profit = tmp.game_result.win == 0 ? -amount : (decimal)tmp.game_result.win - amount,
                            Nonce = long.Parse(tmp.player_seed.Substring(tmp.player_seed.IndexOf("|") + 1)),
                            BetID = tmp.game_id.ToString(),
                            Currency = Currencies[Currency]

                        };
                        resbet.Guid = BetDetails.GUID;
                        Stats.Bets++;
                        lasthash = tmp.server_hash;
                        bool Win = (((bool)High ? (decimal)tmp.game_result.roll > (decimal)MaxRoll - (decimal)(chance) : (decimal)tmp.game_result.roll < (decimal)(chance)));
                        if (Win)
                            Stats.Wins++;
                        else Stats.Losses++;
                        Stats.Wagered += amount;
                        Stats.Profit += resbet.Profit;
                        Stats.Balance = decimal.Parse(CurrencyMap[Currency].ToLower() == "bitcoins" ?
                                tmp.data.balance :
                                CurrencyMap[Currency].ToLower() == "ethers" ? tmp.data.balance_ether
                                : CurrencyMap[Currency].ToLower() == "litecoins" ? tmp.data.balance_litecoin :
                                CurrencyMap[Currency].ToLower() == "dogecoins" ? tmp.data.balance_dogecoin :
                                CurrencyMap[Currency].ToLower() == "bcash" ? tmp.data.balance_bcash : tmp.data.token_balance,
                            System.Globalization.NumberFormatInfo.InvariantInfo);
                        /*tmp.bet.client = tmp.user.client;
                        tmp.bet.serverhash = tmp.user.server;
                        lastupdate = DateTime.Now;
                        balance = tmp.user.balance / 100000000.0m; //i assume
                        bets = tmp.user.bets;
                        wins = tmp.user.wins;
                        losses = tmp.user.losses;
                        wagered = (decimal)(tmp.user.wagered / 100000000m);
                        profit = (decimal)(tmp.user.profit / 100000000m);
                        */
                        callBetFinished(resbet);
                        retrycount = 0;
                    }
                    else
                    {
                        ErrorType type = ErrorType.Unknown;
                        if (tmp.msg == "Invalid target")
                        {
                            type = ErrorType.InvalidBet;
                        }
                        else if (tmp.msg == "Insufficient Funds")
                        {
                            type= ErrorType.BalanceTooLow;
                        }
                        
                        else if (tmp.msg.ToLower() == "bet rate limit exceeded")
                        {
                            /*callNotify(tmp.msg + ". Retrying in a second;");
                            Thread.Sleep(1000);
                            placebetthread(bet);
                            return;*/
                        }
                        else
                        {
                            
                        }
                        callError(tmp.msg, false, type);
                        callNotify(tmp.msg);
                    }
                }
                catch (Exception e)
                {
                    callNotify("An unknown error has occurred");
                    Logger.DumpLog(e);
                }
            }
            catch (AggregateException e)
            {
                if (retrycount++ < 3)
                {
                    Thread.Sleep(500);
                    PlaceDiceBet(BetDetails);
                    return;
                }
                if (e.InnerException.Message.Contains("429") || e.InnerException.Message.Contains("502"))
                {
                    Thread.Sleep(500);
                    PlaceDiceBet(BetDetails);
                }


            }
            catch (Exception e2)
            {

            }
        }

        void GetBalanceThread()
        {

            while (isbv)
            {
                if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 10 || ForceUpdateStats))
                {
                    UpdateStats();
                }
                Thread.Sleep(1000);
            }
        }

        protected override void _UpdateStats()
        {
            try
            {

                {
                    lastupdate = DateTime.Now;
                    ForceUpdateStats = false;
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                    pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                    pairs.Add(new KeyValuePair<string, string>("k", "0"));
                    pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                    pairs.Add(new KeyValuePair<string, string>("u", "0"));
                    pairs.Add(new KeyValuePair<string, string>("self_only", "1"));

                    HttpResponseMessage resp1 = Client.GetAsync("").Result;
                    string s1 = "";
                    if (resp1.IsSuccessStatusCode)
                    {
                        s1 = resp1.Content.ReadAsStringAsync().Result;
                        //Parent.DumpLog("BE login 2.1", 7);
                    }
                    else
                    {
                        //Parent.DumpLog("BE login 2.2", 7);
                        if (resp1.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            s1 = resp1.Content.ReadAsStringAsync().Result;


                            //do cloudflare stuff here


                            /*if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "bitvest.io"))
                            {

                                //finishedlogin(false);
                                return;
                            }*/

                        }
                        //Parent.DumpLog("BE login 2.3", 7);
                    }

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;
                    sEmitResponse = sEmitResponse.Replace("r-", "r_").Replace("n-", "n_");

                    BivestGetBalanceRoot tmpbase = JsonSerializer.Deserialize<BivestGetBalanceRoot>(sEmitResponse);
                    if (tmpbase != null)
                    {
                        if (tmpbase.data != null)
                        {
                            switch (Currency)
                            {
                                //"btc", "tok", "ltc", "eth", "doge","bch" 
                                case 0:
                                    Stats.Balance = (decimal)tmpbase.data.balance; break;
                                case 1:
                                    Stats.Balance = (decimal)tmpbase.data.token_balance; break;
                                case 2:
                                    Stats.Balance = (decimal)tmpbase.data.litecoin_balance; break;
                                case 3: Stats.Balance = (decimal)tmpbase.data.ether_balance; break;
                                case 4: Stats.Balance = (decimal)tmpbase.data.balance_dogecoin; break;
                                case 5: Stats.Balance = (decimal)tmpbase.data.balance_bcash; break;
                                default:
                                    Stats.Balance = (decimal)tmpbase.data.token_balance; break;
                            }


                        }
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
            if (BetDetails is PlaceDiceBet)
            {

                decimal weight = 1;
                if (Currency == 0)
                {
                    switch (CurrencyMap[Currency].ToLower())
                    {
                        /*case "bitcoins": weight = decimal.Parse(Weights.BTC, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "tokens": weight = decimal.Parse(Weights.TOK, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "litecoins": weight = decimal.Parse(Weights.LTC, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "ethers": weight = decimal.Parse(Weights.ETH, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "dogecoins": weight = decimal.Parse(Weights.DOGE, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "bcash": weight = decimal.Parse(Weights.BCH, System.Globalization.NumberFormatInfo.InvariantInfo); break;

                        default: weight = decimal.Parse(Weights.BTC, System.Globalization.NumberFormatInfo.InvariantInfo); break;*/
                    }
                }


                for (int i = Limits.Length - 1; i >= 0; i--)
                {
                    if (i == Limits.Length - 1 && ((decimal)(BetDetails as PlaceDiceBet).Amount * weight) >= (decimal)Limits[i] * 0.00000001m)
                    {
                        return 0;

                    }
                    else if (((decimal)(decimal)(BetDetails as PlaceDiceBet).Amount * weight) >= (decimal)Limits[i] * 0.00000001m)
                    {
                        int timeleft = (int)(((decimal)(DateTime.Now - Lastbet).TotalSeconds - (1.0m / ((decimal)i + 1.0m))) * 1000m);
                        return -timeleft;

                    }
                }
            }

            return 0;
        }

        protected override void _Withdraw(string Address, decimal Amount)
        {
            try
            {

                Thread.Sleep(500);
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("quantity", (Amount).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("address", Address));
                pairs.Add(new KeyValuePair<string, string>("act", "withdraw"));
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("password", pw));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("tfa", ""));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("currency", Currencies[Currency]));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                callWithdrawalFinished(false, "This needs to be fixed. The withdrawal might have succeeded");
                //return true;
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                //return false;
            }
        }
        protected override decimal _GetLucky(string Hash, string ServerSeed, string ClientSeed, int Nonce)
        {
            SHA512 betgenerator = SHA512.Create();
            int charstouse = 5;
            string source = ServerSeed + "|" + ClientSeed;
            byte[] hash = betgenerator.ComputeHash(Encoding.UTF8.GetBytes(source));

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000m;
            }
            return 0;
        }

        protected override void _SendTip(string Username, decimal Amount)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("currency", Currencies[Currency].ToLower()));
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("quantity", Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("act", "send_tip"));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                callTipFinished(false, "This needs to be fixed. The withdrawal might have succeeded");
                // return (sEmitResponse.Contains("true"));
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    callNotify(sEmitResponse);

                }
            }
            //return false;
        }

        protected override void _ResetSeed()
        {

        }

        public class bitvestLoginBase
        {
            public int update_interval { get; set; }
            public bitvestLogin data { get; set; }
            public Chat chat { get; set; }
            public Game game { get; set; }
            public bitvestAccount account { get; set; }
            public double[] rate_limits { get; set; }
            public  string last_user_seed { get; set; }
            public string server_hash { get; set; }
            public bitvesttip tip { get; set; }
        }

        public class bitvestLogin
        {
            public string balance { get; set; }
            public string balance_ether { get; set; }
            public string balance_litecoin { get; set; }
            public string balance_bcash { get; set; }
            public string balance_dogecoin { get; set; }
            public string pending_bitcoin { get; set; }
            public string pending_ether { get; set; }
            public string pending_litecoin { get; set; }
            public string pending_bcash { get; set; }
            public string pending_dogecoin { get; set; }
            public string token_balance { get; set; }
            public string self_username { get; set; }
            public object self_user_id { get; set; }
            public string self_type { get; set; }
            public object self_rank { get; set; }
            public object self_bet_rank { get; set; }
            public string recovery_address { get; set; }
            public Exchange_Rates exchange_rates { get; set; }
            public string context_content { get; set; }
            public string chat_announcement { get; set; }
            public string chat_event_announcement { get; set; }
            public int chatrooms { get; set; }
            public string session_token { get; set; }
            public bool api_enabled { get; set; }
            public bool mystats_private { get; set; }
            public Divest_Lock divest_lock { get; set; }
            
            public string self_total_bet { get; set; }
            public string self_total_bet_ether { get; set; }
            public string self_total_bet_litecoin { get; set; }
            public string self_total_bet_bcash { get; set; }
            public string self_total_bet_dogecoin { get; set; }
            public string self_total_won { get; set; }
            public string self_total_won_ether { get; set; }
            public string self_total_won_litecoin { get; set; }
            public string self_total_won_bcash { get; set; }
            public string self_total_won_dogecoin { get; set; }
            public string self_profit_percent { get; set; }
            public string self_profit_percent_ether { get; set; }
            public string self_profit_percent_litecoin { get; set; }
            public string self_profit_percent_bcash { get; set; }
            public string self_profit_percent_dogecoin { get; set; }
            public string self_total_bets { get; set; }
            public string self_total_bet_roulette { get; set; }
            public string self_total_bet_roulette_ether { get; set; }
            public string self_total_bet_roulette_litecoin { get; set; }
            public string self_total_bet_roulette_bcash { get; set; }
            public string self_total_bet_roulette_dogecoin { get; set; }
            public string self_total_won_roulette { get; set; }
            public string self_total_won_roulette_ether { get; set; }
            public string self_total_won_roulette_litecoin { get; set; }
            public string self_total_won_roulette_bcash { get; set; }
            public string self_total_won_roulette_dogecoin { get; set; }
            public string self_profit_percent_roulette { get; set; }
            public string self_profit_percent_roulette_ether { get; set; }
            public string self_profit_percent_roulette_litecoin { get; set; }
            public string self_profit_percent_roulette_bcash { get; set; }
            public string self_profit_percent_roulette_dogecoin { get; set; }
            public string self_total_bets_roulette { get; set; }
            public string self_total_bet_slot { get; set; }
            public string self_total_bet_slot_ether { get; set; }
            public string self_total_bet_slot_litecoin { get; set; }
            public string self_total_bet_slot_bcash { get; set; }
            public string self_total_bet_slot_dogecoin { get; set; }
            public string self_total_won_slot { get; set; }
            public string self_total_won_slot_ether { get; set; }
            public string self_total_won_slot_litecoin { get; set; }
            public string self_total_won_slot_bcash { get; set; }
            public string self_total_won_slot_dogecoin { get; set; }
            public string self_profit_percent_slot { get; set; }
            public string self_profit_percent_slot_ether { get; set; }
            public string self_profit_percent_slot_litecoin { get; set; }
            public string self_profit_percent_slot_bcash { get; set; }
            public string self_profit_percent_slot_dogecoin { get; set; }
            public string self_total_bets_slot { get; set; }
            public string self_total_bet_bitspin { get; set; }
            public string self_total_bet_bitspin_ether { get; set; }
            public string self_total_bet_bitspin_litecoin { get; set; }
            public string self_total_bet_bitspin_bcash { get; set; }
            public string self_total_bet_bitspin_dogecoin { get; set; }
            public string self_total_won_bitspin { get; set; }
            public string self_total_won_bitspin_ether { get; set; }
            public string self_total_won_bitspin_litecoin { get; set; }
            public string self_total_won_bitspin_bcash { get; set; }
            public string self_total_won_bitspin_dogecoin { get; set; }
            public string self_profit_percent_bitspin { get; set; }
            public string self_profit_percent_bitspin_ether { get; set; }
            public string self_profit_percent_bitspin_litecoin { get; set; }
            public string self_profit_percent_bitspin_bcash { get; set; }
            public string self_profit_percent_bitspin_dogecoin { get; set; }
            public string self_total_bets_bitspin { get; set; }
            public string self_total_bet_dice { get; set; }
            public string self_total_bet_dice_ether { get; set; }
            public string self_total_bet_dice_litecoin { get; set; }
            public string self_total_bet_dice_bcash { get; set; }
            public string self_total_bet_dice_dogecoin { get; set; }
            public string self_total_won_dice { get; set; }
            public string self_total_won_dice_ether { get; set; }
            public string self_total_won_dice_litecoin { get; set; }
            public string self_total_won_dice_bcash { get; set; }
            public string self_total_won_dice_dogecoin { get; set; }
            public string self_profit_percent_dice { get; set; }
            public string self_profit_percent_dice_ether { get; set; }
            public string self_profit_percent_dice_litecoin { get; set; }
            public string self_profit_percent_dice_bcash { get; set; }
            public string self_profit_percent_dice_dogecoin { get; set; }
            public string self_total_bets_dice { get; set; }
            public string self_total_bet_keno { get; set; }
            public string self_total_bet_keno_ether { get; set; }
            public string self_total_bet_keno_litecoin { get; set; }
            public string self_total_bet_keno_bcash { get; set; }
            public string self_total_bet_keno_dogecoin { get; set; }
            public string self_total_won_keno { get; set; }
            public string self_total_won_keno_ether { get; set; }
            public string self_total_won_keno_litecoin { get; set; }
            public string self_total_won_keno_bcash { get; set; }
            public string self_total_won_keno_dogecoin { get; set; }
            public string self_profit_percent_keno { get; set; }
            public string self_profit_percent_keno_ether { get; set; }
            public string self_profit_percent_keno_litecoin { get; set; }
            public string self_profit_percent_keno_bcash { get; set; }
            public string self_profit_percent_keno_dogecoin { get; set; }
            public string self_total_bets_keno { get; set; }
            public string self_total_bet_plinko { get; set; }
            public string self_total_bet_plinko_ether { get; set; }
            public string self_total_bet_plinko_litecoin { get; set; }
            public string self_total_bet_plinko_bcash { get; set; }
            public string self_total_bet_plinko_dogecoin { get; set; }
            public string self_total_won_plinko { get; set; }
            public string self_total_won_plinko_ether { get; set; }
            public string self_total_won_plinko_litecoin { get; set; }
            public string self_total_won_plinko_bcash { get; set; }
            public string self_total_won_plinko_dogecoin { get; set; }
            public string self_profit_percent_plinko { get; set; }
            public string self_profit_percent_plinko_ether { get; set; }
            public string self_profit_percent_plinko_litecoin { get; set; }
            public string self_profit_percent_plinko_bcash { get; set; }
            public string self_profit_percent_plinko_dogecoin { get; set; }
            public string self_total_bets_plinko { get; set; }
            public string self_bits_dropped { get; set; }
            public string self_btc_tickets { get; set; }
            public string self_tok_tickets { get; set; }
            public string level_exp { get; set; }
            public object mod_active_list { get; set; }
            public bool fee_warning { get; set; }
            public string self_ref_count { get; set; }
            public string self_2ref_count { get; set; }
            public string self_3ref_count { get; set; }
            public string self_ref_total_profit { get; set; }
            public string self_ref_total_profit_eth { get; set; }
            public string self_ref_total_profit_ltc { get; set; }
            public string self_ref_total_profit_bch { get; set; }
            public string self_ref_total_profit_doge { get; set; }
            
        }

        public class Exchange_Rates
        {
            public float BCH { get; set; }
            public float BTC { get; set; }
            public float DOGE { get; set; }
            public float ETH { get; set; }
            public float LTC { get; set; }
        }

        public class Divest_Lock
        {
            public bool locked { get; set; }
            public string lock_str { get; set; }
        }

        public class Global_Invest_Margin
        {
            public Bitcoins bitcoins { get; set; }
            public Ethers ethers { get; set; }
            public Litecoins litecoins { get; set; }
            public Bcash bcash { get; set; }
            public Dogecoins dogecoins { get; set; }
        }

        public class Bitcoins
        {
            public string plinko { get; set; }
            public string dice { get; set; }
            public string roulette { get; set; }
            public string bitspin { get; set; }
            public string slot { get; set; }
            public string keno { get; set; }
        }

        public class Ethers
        {
            public string plinko { get; set; }
            public string dice { get; set; }
            public string roulette { get; set; }
            public string bitspin { get; set; }
            public string slot { get; set; }
            public string keno { get; set; }
        }

        public class Litecoins
        {
            public string plinko { get; set; }
            public string dice { get; set; }
            public string roulette { get; set; }
            public string bitspin { get; set; }
            public string slot { get; set; }
            public string keno { get; set; }
        }

        public class Bcash
        {
            public string plinko { get; set; }
            public string dice { get; set; }
            public string roulette { get; set; }
            public string bitspin { get; set; }
            public string slot { get; set; }
            public string keno { get; set; }
        }

        public class Dogecoins
        {
            public string plinko { get; set; }
            public string dice { get; set; }
            public string roulette { get; set; }
            public string bitspin { get; set; }
            public string slot { get; set; }
            public string keno { get; set; }
        }

        public class Chat
        {
            public string max_id { get; set; }
            public object msg { get; set; }
            public object delete { get; set; }
        }

        public class Game
        {
            public long[] max_id { get; set; }
            public object data { get; set; }
        }


        public class DivestLock
        {
            public bool locked { get; set; }
            public string lock_str { get; set; }
        }

        public class GlobalInvestMargin
        {
            public Bitcoins bitcoins { get; set; }
            public Ethers ethers { get; set; }
            public Litecoins litecoins { get; set; }
            public Bcash bcash { get; set; }
            public Dogecoins dogecoins { get; set; }
        }

        public class Tfa
        {
            public bool enabled { get; set; }
            public object tfakey { get; set; }
        }


        public class Account
        {
            public object type { get; set; }
            public object address { get; set; }
            public object address_eth { get; set; }
            public object address_ltc { get; set; }
            public object address_bch { get; set; }
            public object address_doge { get; set; }
            public string secret { get; set; }
        }

        public class Invest
        {
            public object[] active { get; set; }
            public object[] complete { get; set; }
        }


        public class BitVestGetBalance
        {
            public int self_user_id { get; set; }
            public string self_username { get; set; }
            public decimal balance { get; set; }
            public decimal token_balance { get; set; }
            public decimal ether_balance { get; set; }
            public decimal litecoin_balance { get; set; }
            public decimal balance_dogecoin { get; set; }
            public decimal balance_bcash { get; set; }
            public decimal pending { get; set; }
            public decimal ether_pending { get; set; }
            public decimal litecoin_pending { get; set; }
            public string address { get; set; }
            public string ether_address { get; set; }
            public string litecoin_address { get; set; }
            public decimal total_bet { get; set; }
            public decimal total_won { get; set; }
            public decimal total_profit { get; set; }
            public decimal token_total_bet { get; set; }
            public decimal token_total_won { get; set; }
            public decimal token_total_profit { get; set; }
            public decimal ether_total_bet { get; set; }
            public decimal ether_total_won { get; set; }
            public decimal ether_total_profit { get; set; }
            public decimal litecoin_total_bet { get; set; }
            public decimal litecoin_total_won { get; set; }
            public decimal litecoin_total_profit { get; set; }
            public decimal dogecoin_total_bet { get; set; }
            public decimal dogecoin_total_won { get; set; }
            public decimal dogecoin_total_profit { get; set; }
            public decimal bcash_total_bet { get; set; }
            public decimal bcash_total_won { get; set; }
            public decimal bcash_total_profit { get; set; }
            public decimal bets { get; set; }
            public string server_hash { get; set; }
        }

        public class BivestGetBalanceRoot
        {
            public BitVestGetBalance data { get; set; }
        }

        public class bitvesttip
        {
            public bool enabled { get; set; }
        }
        public class bitvestAccount
        {
            public string type { get; set; }
            public string address { get; set; }
            public string secret { get; set; }

        }

        public class bitvestbet
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public bitvestbetdata data { get; set; }
            public bitvestgameresult game_result { get; set; }
            public long game_id { get; set; }
            public string result { get; set; }
            public string server_seed { get; set; }
            public string server_hash { get; set; }
            public string player_seed { get; set; }
        }
        public class bitvestbetdata
        {
            public string balance { get; set; }
            public string pending { get; set; }
            public string balance_ether { get; set; }
            public string token_balance { get; set; }
            public string balance_litecoin { get; set; }
            public string balance_dogecoin { get; set; }
            public string balance_bcash { get; set; }
            public string self_username { get; set; }
            public string self_user_id { get; set; }


        }
        public class bitvestgameresult
        {
            public decimal roll { get; set; }
            public decimal win { get; set; }
            public decimal total_bet { get; set; }
            public decimal multiplier { get; set; }
        }
        public class pdDeposit
        {
            public string address { get; set; }
        }
    }
}
