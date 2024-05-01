using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DoormatCore.Games;
using DoormatCore.Helpers;
using Microsoft.Extensions.Logging;
using WebSocket4Net;

namespace DoormatCore.Sites
{
    class NitrogenSports : BaseSite
    {
        string password = "";
        Dictionary<string, int> Requests = new Dictionary<string, int>();
        string Guid = "";
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool iskd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        string token = "";
        string link = "";
        WebSocket NSSocket = null;

        public NitrogenSports(ILogger logger) : base(logger)
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "NitrogenSports";
            this.SiteName = "NS";
            this.SiteURL = "https://nitrogensports.eu/r/1435541";
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
            this.Currencies = new string[] { "btc"};
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://bitvest.io/bet/{0}";
            this.Edge = 1;
        }
        string CreateRandomString()
        {
            //p4s61ntwgyj5s91igpm0zr529
            int length = 25;
            string s = "";
            string chars = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            while (s.Length < length)
            {
                s += chars[R.Next(0, chars.Length)];
            }
            return s;
        }
        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {
                    if ((DateTime.Now - lastupdate).TotalSeconds > 30)
                    {
                        lastupdate = DateTime.Now;

                        UpdateStats();

                    }
                }
                catch { }
                Thread.Sleep(100);
            }
        }
        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> _Login(LoginParamValue[] LoginParams)
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
            
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://nitrogensports.eu/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");


            try
            {
                string s1 = "";
                HttpResponseMessage resp = await Client.GetAsync("");
                if (resp.IsSuccessStatusCode)
                {
                    s1 = await resp.Content.ReadAsStringAsync();
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = await resp.Content.ReadAsStringAsync();
                        //cflevel = 0;
                        /*
                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "nitrogensports.eu"))
                        {

                            finishedlogin(false);
                            return;
                        }
                        */
                    }
                }
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("captcha_code", ""));
                pairs.Add(new KeyValuePair<string, string>("otp", otp/*==""?"undefined":twofa*/));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                var response = await Client.PostAsync("php/login/login.php", Content);
                string sEmitResponse = await response.Content.ReadAsStringAsync();
                NSLogin tmpLogin = JsonSerializer.Deserialize<NSLogin>(sEmitResponse);
                if (tmpLogin.errno != 0)
                {
                    callLoginFinished(false);
                    return false;

                }
                else
                {
                    Stats.Balance = decimal.Parse(tmpLogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    token = tmpLogin.csrf_token;
                    ConnectSocket();
                    Stats.Balance = decimal.Parse(tmpLogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);

                    await UpdateStats();
                    this.password = Password;
                    return NSSocket.State == WebSocketState.Open;
                }

            }
            catch (AggregateException e)
            {

            }
            catch (Exception e)
            {

            }
            callLoginFinished(false);
            return false;
        }
        void ConnectSocket()
        {
            try
            {
                if (NSSocket != null)
                {
                    try
                    {
                        NSSocket.Close();
                    }
                    catch { }
                }
                string cfclearnace = "";
                string cfuid = "";
                string PHPID = "";
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("setting_name", "dice_sound_on"));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("php/query/account_get_setting.php", Content).Result.Content.ReadAsStringAsync().Result;

                foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://nitrogensports.eu")))
                {
                    switch (c.Name)
                    {
                        case "PHPSESSID": PHPID = c.Value; break;
                        case "__cfduid": cfuid = c.Value; break;
                        case "__cf_bm": cfclearnace = c.Value; break;
                        case "login_link": link = c.Value; break;
                        case "x-csrftoken": token = c.Value; break;

                        default: break;
                    }
                }


                List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                Cookies.Add(new KeyValuePair<string, string>("PHPSESSID", PHPID));
                Cookies.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                Cookies.Add(new KeyValuePair<string, string>("__cf_bm", cfclearnace));
                Cookies.Add(new KeyValuePair<string, string>("login_link", link));
                Cookies.Add(new KeyValuePair<string, string>("x-csrftoken", token));
                //Cookies.Add(new KeyValuePair<string, string>("csrf_token", token));

                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                /*headers.Add(new KeyValuePair<string, string>("Origin", "https://nitrogensports.eu"));
                headers.Add(new KeyValuePair<string, string>("Host", "nitrogensports.eu"));
                headers.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
                headers.Add(new KeyValuePair<string, string>("Connection", "Upgrade"));
                headers.Add(new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate, sdch, br"));
                //Accept-Encoding: gzip, deflate, sdch, br
                //Accept-Language: en-US,en;q=0.8,ru;q=0.6
                headers.Add(new KeyValuePair<string, string>("Accept-Language", "en-US,en;q=0.8,af;q=0.6"));
                //headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Protocol", "wamp"));
                //headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits"));
                //headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Version", "13"));
                /*Sec-WebSocket-Extensions:permessage-deflate; client_max_window_bits
Sec-WebSocket-Key:a0NUCgmYHsEzWIjTfgBuUQ==
Sec-WebSocket-Protocol:actioncable-v1-json
Sec-WebSocket-Version:13*/
                headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0"));

                NSSocket = new WebSocket("wss://nitrogensports.eu/ws/", "wamp", Cookies, headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0", "https://nitrogensports.eu", WebSocketVersion.Rfc6455, null);

                NSSocket.Closed += NSSocket_Closed;
                NSSocket.Error += NSSocket_Error;
                NSSocket.MessageReceived += NSSocket_MessageReceived;
                NSSocket.Opened += NSSocket_Opened;
                NSSocket.Open();
                while (NSSocket.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }
                //CurrencyChanged();
                iskd = true;
                new Thread(new ThreadStart(GetBalanceThread)).Start();
                callLoginFinished(NSSocket.State == WebSocketState.Open);
                return;
                //loggedin = true;


            }
            catch
            {

            }
        }

        void NSSocket_Opened(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
        void NSSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                //Parent.DumpLog(e.Message, -1);
                if (e.Message.StartsWith("[4"))
                {
                    //error!
                    string[] msgs = e.Message.Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
                   callNotify(msgs[msgs.Length - 1]);
                }
                if (e.Message.StartsWith("[3"))
                {
                    //NOT ERROR!

                    //determine what this is
                    string key = e.Message.Substring("[3,\"0.".Length);
                    key = key.Substring(0, key.IndexOf("\""));
                    //get key from request

                    string tmp = e.Message.Substring(e.Message.IndexOf("\",{\"") + 2);
                    tmp = tmp.Substring(0, tmp.Length - 1);

                    if (Requests.ContainsKey(key))
                    {
                        switch (Requests[key])
                        {
                            case 0: processbet(JsonSerializer.Deserialize<NSBet>(tmp)); break;
                            case 1: processStats(JsonSerializer.Deserialize<NSGame>(tmp)); break;
                            case 2: ProcessSeed(JsonSerializer.Deserialize<NSSeed>(tmp)); break;
                        }
                        Requests.Remove(key);
                    }




                    //NSBet tmpbet = 
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.ToString());
            }
            //throw new NotImplementedException();
        }

        private void processStats(NSGame nSGame)
        {
            Stats.Wins = (int)nSGame.betsWon;
            Stats.Losses = (int)nSGame.betsLost;
            Stats.Bets = (int)nSGame.betsMade;
            
        }

        void processbet(NSBet tmpbbet)
        {
            DiceBet newbet = new DiceBet
            {
                BetID = tmpbbet.id,
                TotalAmount = decimal.Parse(tmpbbet.betAmount, System.Globalization.NumberFormatInfo.InvariantInfo),
                DateValue = DateTime.Now,
                ClientSeed = tmpbbet.dice.clientSeed,
                High = tmpbbet.betCondition == "H",
                Chance = tmpbbet.betCondition == "H" ? MaxRoll - decimal.Parse(tmpbbet.betTarget, System.Globalization.NumberFormatInfo.InvariantInfo) : decimal.Parse(tmpbbet.betTarget, System.Globalization.NumberFormatInfo.InvariantInfo),
                Nonce = tmpbbet.nonce,
                Guid = this.Guid,
                Roll = decimal.Parse(tmpbbet.roll, System.Globalization.NumberFormatInfo.InvariantInfo),
                ServerHash = tmpbbet.dice.serverSeedHash
            };
            newbet.Profit = tmpbbet.outcome == "W" ? decimal.Parse(tmpbbet.profitAmount, System.Globalization.NumberFormatInfo.InvariantInfo) : -newbet.TotalAmount;
            if (tmpbbet.outcome == "W")
                Stats.Wins++;
            else
                Stats.Losses++;
            Stats.Bets++;
            Stats.Balance = decimal.Parse(tmpbbet.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
            Stats.Wagered += newbet.TotalAmount;
            Stats.Profit += newbet.Profit;
            callBetFinished(newbet);
        }
        void ProcessSeed(NSSeed tmpSeed)
        {
            //sqlite_helper.InsertSeed(tmpSeed.previousServerSeedHash, tmpSeed.previousServerSeed);
        }
        void NSSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _logger?.LogError(e.Exception.ToString());
            //throw new NotImplementedException();
        }

        void NSSocket_Closed(object sender, EventArgs e)
        {
            try
            {
                callNotify("Connection Lost! Please close NitrogenSports in your browser");
                callError("Connection Lost! Please close NitrogenSports in your browser", true, ErrorType.Other);
            }
            catch { };
            //throw new NotImplementedException();
        }

        protected override async Task<SiteStats> _UpdateStats()
        {
            if (NSSocket != null && NSSocket.State == WebSocketState.Open && iskd)
            {
                string s = CreateRandomString();
                //Requests.Add(s,1);
                NSSocket.Send("[2,\"0." + s + "\",\"ping\",{}]");
                string result = await Client.GetStringAsync("php/login/load_login.php");
                NSLogin tmplogin = JsonSerializer.Deserialize<NSLogin>(result);
                Stats.Balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                Thread.Sleep(1);
                string t = CreateRandomString();
                s = "[2,\"0." + t + "\",\"game\",{}]";
                Requests.Add(t, 1);
                NSSocket.Send(s);

                //wait for the response for the site then return the site stats
                throw new NotImplementedException();
            }
            return null;
            //GetStats();
        }



        public class NSLogin
        {
            public int errno { get; set; }
            public string error { get; set; }
            public int user_id { get; set; }
            public string user_name { get; set; }
            public string login_mode { get; set; }
            public string login_link { get; set; }
            public string bitcoin_address { get; set; }
            public string chat_nickname { get; set; }
            public string odds_format { get; set; }
            public string chat_token { get; set; }
            public string poker_token { get; set; }
            public string balance { get; set; }
            public string inplay { get; set; }
            public string csrf_token { get; set; }

        }
        public class NSSeed
        {
            public string id { get; set; }
            public string clientSeed { get; set; }
            public string serverSeedHash { get; set; }
            public long betsMade { get; set; }
            public string createdAt { get; set; }
            public string userId { get; set; }
            public string previousGameId { get; set; }
            public string previousClientSeed { get; set; }
            public string previousServerSeed { get; set; }
            public string previousServerSeedHash { get; set; }
        }
        public class NSBet
        {
            public string id { get; set; }
            public string betAmount { get; set; }
            public string betCondition { get; set; }
            public string betPayout { get; set; }
            public string betTarget { get; set; }
            public long nonce { get; set; }
            public string roll { get; set; }
            public string outcome { get; set; }
            public string profitAmount { get; set; }
            public string streak { get; set; }
            public string jackpot { get; set; }
            public string createdAt { get; set; }
            public string diceJackpot { get; set; }
            public NSSeed dice { get; set; }
            public string balance { get; set; }

        }

        public class NSGame
        {
            public string id { get; set; }
            public long userId { get; set; }
            public string clientSeed { get; set; }
            public string serverSeedHash { get; set; }
            public bool active { get; set; }
            public long betsMade { get; set; }
            public long betsWon { get; set; }
            public long betsLost { get; set; }
            public string createdAt { get; set; }
            public string previousGameId { get; set; }
            public string previousClientSeed { get; set; }
            public string previousServerSeed { get; set; }
            public string previousServerSeedHash { get; set; }
            public string max_profit_amount { get; set; }
        }
    }
}
