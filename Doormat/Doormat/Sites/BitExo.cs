using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;
using WebSocket4Net;

namespace DoormatCore.Sites
{
    public class BitExo : BaseSite, iDice
    {
        long id = 1;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        WebSocket WSClient;        
        new public static string[] sCurrencies = new string[] { "BTC", "BXO", "CLAM", "DOGE", "LTC" };
        string url = "bit-exo.com";
        public enum ReqType { balance, bet, hash, tip }
        Dictionary<long, ReqType> Requests = new Dictionary<long, ReqType>();
        string guid = "";
        string clientseed = "";
        string ServerHash = "";
        bool High;
        decimal Chance;

        public BitExo()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", false, true, false, false)};
            this.MaxRoll = 99.9999m;
            this.SiteAbbreviation = "BE";
            this.SiteName = "Bit-Exo";
            this.SiteURL = "https://bit-exo.com/?ref=seuntjie";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = false;
            this.CanChangeSeed = false;
            this.CanChat = false;
            this.CanGetSeed = false;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = true;
            this.CanVerify = false;
            this.Currencies = new string[] { "BTC", "BXO", "CLAM", "DOGE", "LTC" };            
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://bit-exo.com/{0}";
            this.Edge = 1;
        }


        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            ispd = false;
            if (WSClient != null)
            {
                try
                {
                    WSClient.Close();
                }
                catch { };
            }
        }
        CookieContainer cookies = new CookieContainer();
        protected override void _Login(LoginParamValue[] LoginParams)
        {
            
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip};
            ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://" + url + "/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            Logger.DumpLog("BE login 1", 8);

            string APIKey = "";
            foreach (LoginParamValue x in LoginParams)
            {
                if (x.Param.Name.ToLower() == "api key")
                    APIKey = x.Value;
            }
            try
            {
                accesstoken = APIKey;
                ConnectSocket();
                if (WSClient.State == WebSocketState.Open)
                {
                    Logger.DumpLog("BE login 7.1", 7);
                    ispd = true;

                    lastupdate = DateTime.Now;
                    ForceUpdateStats = false;
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

        private void ConnectSocket()
        {
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
            string chatinit = "42" + id++ + "[\"access_token_data\",{\"access_token\":\"" + accesstoken + "\"}]";
            chatinit = chatinit.Length + ":" + chatinit;
            var content = new StringContent(chatinit, Encoding.UTF8, "application/octet-stream");
            //response = Client.PostAsync("socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate() + "&sid=" + c, content).Result.Content.ReadAsStringAsync().Result;
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
        }

        bool FinishedLogin = false;

        protected override void _UpdateStats()
        {
            if (FinishedLogin)
            {
                ForceUpdateStats = false;
                lastupdate = DateTime.Now;
                
                long tmpid = id++;
                Requests.Add(tmpid, ReqType.balance);
                WSClient.Send("42" + tmpid + "[\"access_token_data\",{\"access_token\":\"" + accesstoken + "\"}]");
            }
        }
        private void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if (WSClient.State == WebSocketState.Open && ((DateTime.Now - lastupdate).TotalSeconds > 15 || ForceUpdateStats))
                    {
                        WSClient.Send("2");
                        UpdateStats();
                    }
                }
                catch
                {

                }
                Thread.Sleep(1000);
            }
        }
        private void WSClient_Opened(object sender, EventArgs e)
        {
            WSClient.Send("2probe");
        }

        private void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Logger.DumpLog(e.Message, -1);
            if (e.Message == "3probe")
            {
                WSClient.Send("5");
                
                long tmpid = this.id++;
                
                Requests.Add(tmpid, ReqType.balance);
                WSClient.Send("42" + tmpid + "[\"access_token_data\",{\"access_token\":\"" + accesstoken + "\"}]");
                tmpid = this.id++;
                Thread.Sleep(200);
                Requests.Add(tmpid, ReqType.hash);
                WSClient.Send("42" + tmpid + "[\"get_hash\"]");
                FinishedLogin = true;

            }
            else if (e.Message == "3")
            {
            }
            else
            {
                try
                {
                    string response = e.Message;
                    response = response.Substring(2);
                    string id = response.Substring(0, response.IndexOf("["));
                    long lid = 0;

                    if (long.TryParse(id, out lid))
                    {
                        if (Requests.ContainsKey(lid))
                        {
                            ReqType tmp = Requests[lid];
                            Requests.Remove(lid);
                            switch (tmp)
                            {
                                case ReqType.balance:
                                    response = response.Substring(response.IndexOf("{"));
                                    response = response.Substring(0, response.LastIndexOf("}") + 1); ProcessBalance(response); break;
                                case ReqType.hash:
                                    response = response.Substring(response.IndexOf("\"") + 1);
                                    response = response.Substring(0, response.LastIndexOf("\"")); ProcessHash(response); break;
                                case ReqType.bet:
                                    if (response.IndexOf("{") >= 0)
                                    {
                                        response = response.Substring(response.IndexOf("{"));
                                        response = response.Substring(0, response.LastIndexOf("}") + 1); ProcessBet(response);
                                    }
                                    else
                                    {
                                        //Parent.updateStatus(response);
                                        if (response.Contains("HASH ERROR") || response.Contains("HASH NOT FOUND"))
                                        {
                                            long tmpid = this.id++;
                                            Requests.Add(tmpid, ReqType.balance);
                                            WSClient.Send("42" + tmpid + "[\"get_hash\"]");
                                        }

                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.DumpLog(ex);
                }
            }

        }

        

        protected override void _SendTip(string User, decimal amount)
        {
            long tmpid = id++;
            Requests.Add(tmpid, ReqType.tip);
            //426["send_tip",{"uname":"professor","amount":1000,"private":false,"type":"BTC"}]
            string request = string.Format("42{3}[\"send_tip\",{{\"uname\":\"{0}\",\"amount\":{1},\"private\":false,\"type\":\"{2}\"}}]",
               User,
                Math.Floor(amount * 100000000m),
                CurrentCurrency,
                tmpid);
            WSClient.Send(request);
            ForceUpdateStats = true;
            //return true;
        }

        void ProcessBalance(string Res)
        {
            BEBalanceBase tmpResult = json.JsonDeserialize<BEBalanceBase>(Res);
            switch (CurrentCurrency.ToLower())
            {
                case "btc":
                    Stats.Balance = tmpResult.user.balances.btc / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.btc.bets;
                    Stats.Wagered = tmpResult.user.stats.btc.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.btc.profit / 100000000m;
                    break;
                case "bxo":
                    Stats.Balance = tmpResult.user.balances.bxo / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.bxo.bets;
                    Stats.Wagered = tmpResult.user.stats.bxo.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.bxo.profit / 100000000m;
                    break;
                case "clam":
                    Stats.Balance = tmpResult.user.balances.clam / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.clam.bets;
                    Stats.Wagered = tmpResult.user.stats.clam.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.clam.profit / 100000000m;
                    break;
                case "doge":
                    Stats.Balance = tmpResult.user.balances.doge / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.doge.bets;
                    Stats.Wagered = tmpResult.user.stats.doge.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.doge.profit / 100000000m;
                    break;
                case "eth":
                    Stats.Balance = tmpResult.user.balances.eth / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.eth.bets;
                    Stats.Wagered = tmpResult.user.stats.eth.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.eth.profit / 100000000m;
                    break;
                case "flash":
                    Stats.Balance = tmpResult.user.balances.flash / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.flash.bets;
                    Stats.Wagered = tmpResult.user.stats.flash.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.flash.profit / 100000000m;
                    break;
                case "ltc":
                    Stats.Balance = tmpResult.user.balances.ltc / 100000000m;
                    Stats.Bets = (int)tmpResult.user.stats.ltc.bets;
                    Stats.Wagered = tmpResult.user.stats.ltc.wager / 100000000m;
                    Stats.Profit = tmpResult.user.stats.ltc.profit / 100000000m;
                    break;
            }            
        }

        void ProcessHash(string Res)
        {
            this.ServerHash = Res;
        }

        void ProcessBet(string Res)
        {
            //433[null,{"created_at":"2019-01-19T11:47:37.991Z","raw_outcome":4264578078,"uname":"seuntjie","secret":2732336931,"salt":"7e59aAbbf531c9649a89a761ac96aba8","hash":"8cf7f15c173d47020d99b20bfa65f4b438799254b69ea8d208aba7fb4dc1c244","client_seed":1532241147,"payouts":[{"from":2168956358,"to":4294967295,"value":2}],"wager":1,"profit":1,"kind":"DICE","edge":1,"ref":null,"currency":"BXO","_id":3917670,"id":3917670,"next_hash":"dc407a52731281f7d9d32eb4e3dff3c84828b4284071483f9826a3072085261c","outcome":99.2923}]
            BEBetResult tmpResult = json.JsonDeserialize<BEBetResult>(Res);
            DiceBet newBet = new DiceBet
            {
                TotalAmount = ((decimal)tmpResult.wager) / 100000000m,
                DateValue = DateTime.Now,
                ClientSeed = tmpResult.client_seed.ToString(),
                Currency = CurrentCurrency,
                Guid = guid,
                High = this.High,
                Nonce = -1,
                BetID = tmpResult.id.ToString(),
                Roll = (decimal)tmpResult.outcome,
                ServerHash = ServerHash,
                ServerSeed = tmpResult.secret + "-" + tmpResult.salt,
                Profit = ((decimal)tmpResult.profit) / 100000000m,
                Chance = Chance
            };
            Stats.Bets++;
            Stats.Wagered += newBet.TotalAmount;
            Stats.Balance += newBet.Profit;
            Stats.Profit += newBet.Profit;
            if ((newBet.High && newBet.Roll > MaxRoll - newBet.Chance) || (!newBet.High && newBet.Roll < newBet.Chance))
            {
                Stats.Wins++;
            }
            else
                Stats.Losses++;
            this.ServerHash = tmpResult.next_hash;
            callBetFinished(newBet);
        }

        private void WSClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Logger.DumpLog(e.Exception);
        }

        private void WSClient_DataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        private void WSClient_Closed(object sender, EventArgs e)
        {
            Logger.DumpLog("BE socket closed", 1);
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                if (WSClient.State != WebSocketState.Open && ispd)
                {
                    callNotify("Attempting Reconnect");
                    Logger.DumpLog("Reconnecting Bit-Exo socket", 2);
                    ConnectSocket();
                    Thread.Sleep(1000);
                }
                this.guid = BetDetails.GUID;
                clientseed = R.Next(0, int.MaxValue).ToString();
                long tmpid = id++;
                this.High = BetDetails.High;
                Chance = BetDetails.Chance;
                Requests.Add(tmpid, ReqType.bet);
                string request = string.Format("42{7}[\"dice_bet\",{{\"wager\":{0:0},\"client_seed\":{1},\"hash\":\"{2}\",\"cond\":\"{3}\",\"target\":{4},\"payout\":{5},\"currency\":\"{6}\"}}]",
                    Math.Floor(BetDetails.Amount * 100000000m),
                    clientseed,
                    ServerHash,
                    High ? ">" : "<",
                    High ? MaxRoll - Chance : Chance,
                    Math.Floor((BetDetails.Amount * 100000000m)) * ((100 - Edge) / Chance),
                    CurrentCurrency,
                    tmpid);
                WSClient.Send(request);
            }
            catch (Exception e)
            {

            }
        }

        public class BEPayout
        {
            public long from { get; set; }
            public long to { get; set; }
            public decimal value { get; set; }
        }

        public class BEBetResult
        {
            public string created_at { get; set; }
            public long raw_outcome { get; set; }
            public string uname { get; set; }
            public long secret { get; set; }
            public string salt { get; set; }
            public string hash { get; set; }
            public long client_seed { get; set; }
            public List<BEPayout> payouts { get; set; }
            public long wager { get; set; }
            public decimal profit { get; set; }
            public string kind { get; set; }
            public decimal edge { get; set; }
            public object @ref { get; set; }
            public string currency { get; set; }
            public long _id { get; set; }
            public long id { get; set; }
            public string next_hash { get; set; }
            public decimal outcome { get; set; }
        }
        public class BEBalances
        {
            public decimal doge { get; set; }
            public decimal bxo { get; set; }
            public decimal clam { get; set; }
            public decimal btc { get; set; }
            public decimal eth { get; set; }
            public decimal ltc { get; set; }
            public decimal flash { get; set; }
        }
        public class BECoininf
        {
            public decimal bets { get; set; }
            public decimal wager { get; set; }
            public decimal profit { get; set; }
            public decimal wager24hour { get; set; }
        }

        public class BEStats
        {
            public BECoininf btc { get; set; }
            public BECoininf bxo { get; set; }
            public BECoininf clam { get; set; }
            public BECoininf doge { get; set; }
            public BECoininf eth { get; set; }
            public BECoininf flash { get; set; }
            public BECoininf ltc { get; set; }
        }
        public class BEUser
        {
            public string uname { get; set; }
            public string email { get; set; }
            public BEBalances balances { get; set; }
            public BEStats stats { get; set; }

            public string last_claim_time { get; set; }
            public double last_claim_betted_wager { get; set; }
            public double total_claims { get; set; }
            public string token { get; set; }
            public double level { get; set; }
            public double levelwager { get; set; }
        }

        public class BEBalanceBase
        {
            public BEUser user { get; set; }
        }
    }
}
