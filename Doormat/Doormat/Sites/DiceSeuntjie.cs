using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;
using WebSocket4Net;

namespace DoormatCore.Sites
{
    class DiceSeuntjie : BaseSite
    {
        PlaceDiceBet lastbet = null;

        public int APPId { get; set; }
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        WebSocket WSClient;// = new WebSocket("");
        RandomNumberGenerator R = new System.Security.Cryptography.RNGCryptoServiceProvider();
        public static string[] sCurrencies = new string[] { "BTC" };//,"LTC","DASH" };

        public DiceSeuntjie()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", true, true, false, true) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "EC";
            this.SiteName = "DS";
            this.SiteURL = "https://Dice.Seuntjie.com";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = false;
            this.CanChangeSeed = false;
            this.CanChat = false;
            this.CanGetSeed = false;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = false;
            this.CanVerify = false;
            this.Currencies = sCurrencies;
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://Dice.Seuntjie.com/{0}";
            this.Edge = 0.9m;
        }
        protected string url { get; set; }
        long id = 0;
        string OldHash = "";
        string ClientSeed = "";
        string Guid = "";
        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            ispd = false;
            try
            {
                WSClient.Close();
            }
            catch { }
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            foreach (LoginParamValue x in LoginParams)
            {
                if (x.Param.Name.ToLower() == "api key")
                    accesstoken = x.Value;

            }
            CookieContainer cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip};
            ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://" + url + "/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            Logger.DumpLog("BE login 1", 8);

            try
            {
                //accesstoken = Password;
                string s1 = "";
                HttpResponseMessage resp = Client.GetAsync("").Result;
                Logger.DumpLog("BE login 2", 8);
                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                    Logger.DumpLog("BE login 2.1", 7);
                }
                else
                {
                    Logger.DumpLog("BE login 2.2", 7);
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                        /*
                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "bit-exo.com"))
                        {

                            finishedlogin(false);
                            return;
                        }*/

                    }
                    Logger.DumpLog("BE login 2.3", 7);
                }
                string response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate()).Result;
                Logger.DumpLog("BE login 3", 7);
                string c =
                response.Substring(response.IndexOf("sid\":\"") + "sid\":\"".Length);
                c = c.Substring(0, c.IndexOf("\""));
                Logger.DumpLog("BE login 4", 7);
                foreach (Cookie c3 in cookies.GetCookies(new Uri("http://" + url)))
                {
                    if (c3.Name == "io")
                        c = c3.Value;
                    /*if (c3.Name == "__cfduid")
                        c2 = c3;*/
                }
                Logger.DumpLog("BE login 5", 7);
                string chatinit = "420[\"chat_init\",{\"app_id\":" + APPId + ",\"access_token\":\"" + accesstoken + "\",\"subscriptions\":[\"CHAT\",\"DEPOSITS\",\"BETS\"]}]";
                chatinit = chatinit.Length + ":" + chatinit;
                var content = new StringContent(chatinit, Encoding.UTF8, "application/octet-stream");
                response = Client.PostAsync("socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate() + "&sid=" + c, content).Result.Content.ReadAsStringAsync().Result;
                Logger.DumpLog("BE login 5", 7);
                List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
                Headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"));
                foreach (Cookie x in cookies.GetCookies(new Uri("https://" + url)))
                {
                    Cookies.Add(new KeyValuePair<string, string>(x.Name, x.Value));
                }
                Cookies.Add(new KeyValuePair<string, string>("io", c));
                Logger.DumpLog("BE login 6", 7);
                WSClient = new WebSocket("wss://" + url + "/socket.io/?EIO=3&transport=websocket&sid=" + c, null, Cookies, Headers, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36", "https://" + url, WebSocketVersion.Rfc6455, null, System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);
                WSClient.Closed += WSClient_Closed;
                WSClient.DataReceived += WSClient_DataReceived;
                WSClient.Error += WSClient_Error;
                WSClient.MessageReceived += WSClient_MessageReceived;
                WSClient.Opened += WSClient_Opened;
                WSClient.Open();
                while (WSClient.State == WebSocketState.Connecting)
                    Thread.Sleep(100);
                if (WSClient.State == WebSocketState.Open)
                {
                    Logger.DumpLog("BE login 7.1", 7);
                    ispd = true;

                    lastupdate = DateTime.Now;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    callLoginFinished(true); return;
                }
                else
                {
                    Logger.DumpLog("BE login 7.2", 7);
                    callLoginFinished(false);
                    return;
                }
            }
            catch (AggregateException ER)
            {
                Logger.DumpLog(ER.ToString(), -1);
                callLoginFinished(false);
                return;
            }
            catch (Exception ERR)
            {
                Logger.DumpLog(ERR.ToString(), -1);
                callLoginFinished(false);
                return;
            }
            callLoginFinished(false);
            return;
        }
        void GetBalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds >= 30)
                {
                    lastupdate = DateTime.Now;
                    UpdateStats();

                }
                Thread.Sleep(500);
            }
        }
        protected override void _UpdateStats()
        {
            lastupdate = DateTime.Now;
            string Bet = string.Format(
        System.Globalization.NumberFormatInfo.InvariantInfo,
        "42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\",\"currency\":\"{3}\"}}]",
        id++,
        APPId,
        accesstoken,
        Currency
        );
            WSClient.Send(Bet);
            WSClient.Send("2");

        }
        void WSClient_Opened(object sender, EventArgs e)
        {
            WSClient.Send("2probe");
            Logger.DumpLog("opened", 1);
        }

        void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Logger.DumpLog(e.Message, 10);
            if (e.Message == "3probe")
            {
                WSClient.Send("5");
                WSClient.Send("42" + id++ + "[\"get_hash\",null]");
                string Bet = string.Format(
                System.Globalization.NumberFormatInfo.InvariantInfo,
                "42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\",\"currency\":\"{3}\"}}]",
                id++,
                APPId,
                accesstoken,
                Currency
                );
                WSClient.Send(Bet);

            }
            else
            {
                if (e.Message.Contains("[null,{\"bet_hash\":"))
                {
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDiceHash tmphash = json.JsonDeserialize<SDiceHash>(msg);
                    this.OldHash = tmphash.bet_hash;
                }
                if (e.Message.Contains("[null,{\"token\":"))
                {
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDIceAccToken tmphash = json.JsonDeserialize<SDIceAccToken>(msg);
                    Stats.Balance = (CurrentCurrency.ToLower() == "btc" ? tmphash.user.balances.btc :
                        CurrentCurrency.ToLower() == "ltc" ? tmphash.user.balances.ltc :
                        CurrentCurrency.ToLower() == "dash" ? tmphash.user.balances.dash :
                        0
                        ) / 100000000m;
                    Stats.Profit = tmphash.user.betted_profit / 100000000m;
                    Stats.Wagered = tmphash.user.betted_wager / 100000000m;
                    Stats.Bets = (int)tmphash.user.betted_count;
                    
                }
                if (e.Message.Contains("[null,{\"auth_id\":"))
                {
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    sdiceauth tmphash = json.JsonDeserialize<sdiceauth>(msg);
                    Stats.Balance = (CurrentCurrency.ToLower() == "btc" ? tmphash.user.balances.btc :
                        CurrentCurrency.ToLower() == "ltc" ? tmphash.user.balances.ltc :
                        CurrentCurrency.ToLower() == "dash" ? tmphash.user.balances.dash :
                        0
                        ) / 100000000m;
                    Stats.Profit = tmphash.user.betted_profit / 100000000m;
                    Stats.Wagered = tmphash.user.betted_wager / 100000000m;
                    Stats.Bets = (int)tmphash.user.betted_count;
                    
                }
                if (e.Message.Contains("[null,{\"id\":"))
                {
                    //do bet
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDIceBet tmphash = json.JsonDeserialize<SDIceBet>(msg);
                    if (tmphash.bet_id > 0)
                    {
                        
                        DiceBet newbet = new DiceBet
                        {
                            TotalAmount = lastbet.Amount,
                            DateValue = DateTime.Now,
                            Chance = lastbet.Chance,
                            High = lastbet.High,
                            ClientSeed = ClientSeed,
                            BetID = tmphash.bet_id.ToString(),
                            Nonce = 0,
                            ServerSeed = tmphash.secret,//tmphash.salt,
                            ServerHash = OldHash,
                            Profit = tmphash.profit / 100000000m,
                            Roll = decimal.Parse(tmphash.outcome, System.Globalization.NumberFormatInfo.InvariantInfo),
                            Guid = this.Guid
                        };
                        OldHash = tmphash.next_hash;
                        Stats.Balance += newbet.Profit;
                        Stats.Profit += newbet.Profit;
                        Stats.Bets++;
                        Stats.Wagered += newbet.TotalAmount;
                        bool win = false;
                        if (newbet.GetWin(this))
                        {
                            win = true;
                        }
                        if (win)
                           Stats.Wins++;
                        else
                            Stats.Losses++;
                        callBetFinished(newbet);
                    }
                }
            }
        }

        void WSClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Logger.DumpLog("error", -1);
            Logger.DumpLog(e.Exception.ToString(), -1);
        }

        void WSClient_DataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        void WSClient_Closed(object sender, EventArgs e)
        {
            Logger.DumpLog("closed", -1);
        }

        protected override void _PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            this.Guid = Guid;
            ClientSeed = "";
            byte[] bytes = new byte[4];
            R.GetBytes(bytes);
            long client = (long)BitConverter.ToUInt32(bytes, 0);
            ClientSeed = client.ToString();
            long Roundedamount = (long)(Math.Round((BetDetails.Amount), 8) * 100000000);
            lastbet= BetDetails;
            string Bet = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "42{0}[\"dice_bet\",{{\"wager\":{1:0},\"client_seed\":{2},\"hash\":\"{3}\",\"cond\":\"{4}\",\"target\":{5:0.0000},\"payout\":{6:0.0000}, \"currency\":\"{7}\"}}]",
                id++, Roundedamount, ClientSeed, OldHash, BetDetails.High ? ">" : "<", BetDetails.High ? 100m - BetDetails.Chance : BetDetails.Chance, (((100m - Edge) / BetDetails.Chance) * Roundedamount), Currency);
            WSClient.Send(Bet);
        }




        public class SDIceBet
        {
            public long ID { get; set; }
            public long bet_id { get; set; }
            public string outcome { get; set; }
            public decimal profit { get; set; }
            public string secret { get; set; }
            public string salt { get; set; }
            public string created_at { get; set; }
            public string next_hash { get; set; }
            public long raw_outcome { get; set; }
            public string kind { get; set; }
        }
        public class SDIceAccToken
        {
            public SDIceAccToken auth { get; set; }
            public SDiceUser user { get; set; }
        }

        public class SDiceUser
        {
            public string uname { get; set; }
            public string role { get; set; }
            public sdiceBalances balances { get; set; }
            public sdiceBalances unconfirmed { get; set; }
            public object balance { get; set; }
            public decimal wager24hour { get; set; }
            public decimal profit24hour { get; set; }
            public object @ref { get; set; }
            public decimal refprofit { get; set; }
            public decimal refpaid { get; set; }
            public decimal refprofitLTC { get; set; }
            public decimal refprofitDASH { get; set; }
            public decimal refprofitDOGE { get; set; }
            public List<string> open_pm { get; set; }
            public decimal betted_wager { get; set; }
            public decimal betted_count { get; set; }
            public decimal betted_profit { get; set; }
            public decimal level { get; set; }
        }
        public class SDiceHash
        {
            public string hash { get; set; }
            public string bet_hash { get; set; }
        }
        public class sdiceBalances
        {
            public decimal btc { get; set; }
            public decimal ltc { get; set; }
            public decimal dash { get; set; }
        }
        public class sdiceauth
        {
            public sdiceauth auth { get; set; }
            public int id { get; set; }
            public int auth_id { get; set; }
            public int app_id { get; set; }
            public SDiceUser user { get; set; }
        }
    }
}
