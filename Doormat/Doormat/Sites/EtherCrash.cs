using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DoormatCore.Games;
using DoormatCore.Helpers;
using WebSocket4Net;

namespace DoormatCore.Sites
{
    class EtherCrash : BaseSite, iCrash
    {
        PlaceCrashBet LastBet = null;
        string Token = "";
        CookieContainer Cookies;
        HttpClientHandler ClientHandlr;
        HttpClient Client;
        WebSocket Sock;
        WebSocket ChatSock;
        bool isec = false;
        string username = "";
        string io = "";
        string cfuid = "";

        public EtherCrash()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", true, true, false, true) };
            this.MaxRoll = 99.99999m;
            this.SiteAbbreviation = "EC";
            this.SiteName = "EtherCrash";
            this.SiteURL = "https://EtherCrash.io";
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
            this.Currencies = new string[] {"Eth"};
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://EtherCrash.io/{0}";
            this.Edge = 1;
        }
        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            if (Sock != null)
            {
                try
                {
                    isec = false;
                    Sock.Close();
                }
                catch
                {

                }
            }
        }

        protected override void _Disconnect()
        {
            throw new NotImplementedException();
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            string APIKey = "";
            foreach (LoginParamValue x in LoginParams)
            {
                if (x.Param.Name.ToLower() == "api key")
                    APIKey = x.Value;

            }
            Cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = Cookies, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }; ;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://ethercrash.io") };
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
            Client.DefaultRequestHeaders.Add("referer", "https://www.ethercrash.io/play");
            Client.DefaultRequestHeaders.Add("origin", "https://www.ethercrash.io");
            try
            {
                Cookies.Add(new Cookie("id", APIKey, "/", "ethercrash.io"));


                string Response = Client.GetStringAsync("https://www.ethercrash.io/play").Result;


                Response = Client.GetStringAsync("https://www.ethercrash.io/socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate()).Result;

                Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate()).Result;

                string iochat = "";
                foreach (Cookie c3 in Cookies.GetCookies(new Uri("http://www.ethercrash.io")))
                {
                    if (c3.Name == "io")
                        iochat = c3.Value;
                    if (c3.Name == "__cfduid")
                        cfuid = c3.Value;
                }
                Response = Client.GetStringAsync("https://www.ethercrash.io/socket.io/?EIO=3&sid=" + iochat + "&transport=polling&t=" + json.CurrentDate()).Result;


                foreach (Cookie c3 in Cookies.GetCookies(new Uri("http://gs.ethercrash.io")))
                {
                    if (c3.Name == "io")
                        io = c3.Value;
                    if (c3.Name == "__cfduid")
                        cfuid = c3.Value;
                }

                StringContent ottcontent = new StringContent("");
                HttpResponseMessage RespMsg = Client.PostAsync("https://www.ethercrash.io/ott", ottcontent).Result;

                Response = RespMsg.Content.ReadAsStringAsync().Result;
                if (RespMsg.IsSuccessStatusCode)
                    ott = Response;
                else
                {
                    callLoginFinished(false);
                    return;
                }


                string body = "420[\"join\",{\"ott\":\"" + ott + "\",\"api_version\":1}]";
                body = body.Length + ":" + body;
                StringContent stringContent = new StringContent(body, UnicodeEncoding.UTF8, "text/plain");

                Response = Client.PostAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate(), stringContent).Result.Content.ReadAsStringAsync().Result;




                body = "420[\"join\",[\"english\"]]";
                body = body.Length + ":" + body;
                StringContent stringContent2 = new StringContent(body, UnicodeEncoding.UTF8, "text/plain");

                Response = Client.PostAsync("https://www.ethercrash.io/socket.io/?EIO=3&sid=" + iochat + "&transport=polling&t=" + json.CurrentDate(), stringContent2).Result.Content.ReadAsStringAsync().Result;

                Response = Client.GetStringAsync("https://www.ethercrash.io/socket.io/?EIO=3&sid=" + iochat + "&transport=polling&t=" + json.CurrentDate()).Result;

                List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>();
                cookies.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                cookies.Add(new KeyValuePair<string, string>("io", iochat));
                cookies.Add(new KeyValuePair<string, string>("id", APIKey));

                ChatSock = new WebSocket("wss://www.ethercrash.io/socket.io/?EIO=3&transport=websocket&sid=" + iochat,
                   null,
                   cookies/*,
                   null,
                   "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36",
                   "https://www.ethercrash.io",
                    WebSocketVersion.None*/);
                ChatSock.Opened += Sock_Opened;
                ChatSock.Error += Sock_Error;
                ChatSock.MessageReceived += Sock_MessageReceived;
                ChatSock.Closed += Sock_Closed;
                ChatSock.Open();
                while (ChatSock.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(300);
                    //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                }
                if (ChatSock.State == WebSocketState.Open)
                {
                }
                else
                {
                    callLoginFinished(false);
                    return;
                }


                //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                Thread.Sleep(200);
                //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                List<KeyValuePair<string, string>> cookies2 = new List<KeyValuePair<string, string>>();
                cookies2.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                cookies2.Add(new KeyValuePair<string, string>("io", io));
                cookies2.Add(new KeyValuePair<string, string>("id", APIKey));

                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                headers.Add(new KeyValuePair<string, string>("Host", "gs.ethercrash.io"));
                headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36"));
                headers.Add(new KeyValuePair<string, string>("Origin", "https://www.ethercrash.io"));

                Sock = new WebSocket("wss://gs.ethercrash.io/socket.io/?EIO=3&transport=websocket&sid=" + io/*,
                    null,
                    cookies2,
                    headers,
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36",
                    "https://www.ethercrash.io",
                        WebSocketVersion.None*/);
                Sock.Opened += Sock_Opened;
                Sock.Error += Sock_Error;
                Sock.MessageReceived += Sock_MessageReceived;
                Sock.Closed += Sock_Closed;
                Sock.Open();
                while (Sock.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(300);
                    //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                }
                if (Sock.State == WebSocketState.Open)
                {
                    callLoginFinished(true);
                    isec = true;
                    Thread t = new Thread(pingthread);
                    t.Start();
                    return;
                }
                callLoginFinished(false);
            }
            catch (Exception ex)
            {
                Logger.DumpLog(ex.ToString(), -1);
                callLoginFinished(false);
            }

        }

        protected override void _UpdateStats()
        {
            throw new NotImplementedException();
        }


        DateTime LastPing = DateTime.Now;
        void pingthread()
        {
            while (isec)
            {
                if ((DateTime.Now - LastPing).TotalSeconds >= 15)
                {
                    LastPing = DateTime.Now;
                    try
                    {
                        Sock.Send("2");
                        ChatSock.Send("2");
                    }
                    catch
                    {
                        isec = false;
                    }
                }
            }
        }

        private void Sock_Closed(object sender, EventArgs e)
        {

        }

        string ott = "";
        bool betsOpen;
        bool cashedout = false;
        string GameId = "";
        private void Sock_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message == "3probe")
            {
                (sender as WebSocket).Send("5");
                //Sock.Send("420[\"join\",{\"ott\":\"" + ott + "\",\"api_version\":1}]");     
            }
            else
            {
                Logger.DumpLog(e.Message, -1);
                if (e.Message.StartsWith("42[\"game_starting\","))
                {
                    //42["game_starting",{"game_id":214035,"max_win":3782817516,"time_till_start":5000}]
                    string id = e.Message.Substring(e.Message.IndexOf("\"game_id\":") + "\"game_id\":".Length);
                    id = id.Substring(0, id.IndexOf(","));
                    GameId = id;
                    cashedout = false;
                    this.guid = "";
                    //open betting for user
                    betsOpen = true;
                    callNotify("Game starting - waiting for bet");
                }
                else if (e.Message.StartsWith("42[\"game_started\","))
                {
                    //close betting and wait for result
                    betsOpen = false;
                }
                else if (e.Message.StartsWith("42[\"game_crash\",") && guid != "")
                {
                    //if not cashed out yet, it's a loss and debit balance

                    CrashBet bet = new CrashBet
                    {
                        TotalAmount = (decimal)LastBet.TotalAmount,
                        Profit = -(decimal)LastBet.TotalAmount,                        
                        Currency = CurrentCurrency,
                        DateValue = DateTime.Now,
                        BetID = GameId != "" ? GameId : Guid.NewGuid().ToString(),                        
                        Guid = guid,
                        Payout = LastBet.Payout,
                        Crash = 0//get crash payout from game message
                    };

                    Stats.Balance -= (decimal)LastBet.TotalAmount;
                    Stats.Profit -= (decimal)LastBet.TotalAmount;
                    Stats.Wagered += (decimal)LastBet.TotalAmount;
                    Stats.Losses++;
                    Stats.Bets++;
                    guid = "";
                    callBetFinished(bet);
                    //Parent.updateStatus("Game crashed - Waiting for next game");
                }
                else if (e.Message.StartsWith("42[\"cashed_out\"") && guid != "")
                {
                    //check if the cashed out user is the current user, if it is, it's a win.

                    if (e.Message.Contains("\"" + username + "\""))
                    {
                        cashedout = true;
                        CrashBet bet = new CrashBet
                        {
                            TotalAmount = (decimal)LastBet.TotalAmount,
                            Profit = LastBet.Payout * LastBet.TotalAmount - LastBet.TotalAmount,                            
                            Currency = CurrentCurrency,
                            DateValue = DateTime.Now,
                            BetID = GameId != "" ? GameId : Guid.NewGuid().ToString(),
                            Guid = guid,
                            Payout = LastBet.Payout,

                        };
                        this.guid = "";
                        Stats.Balance += bet.Profit;
                        Stats.Profit += bet.Profit;
                        Stats.Wagered += LastBet.TotalAmount;
                        Stats.Wins++;
                        Stats.Bets++;
                        callBetFinished(bet);
                    }
                }
                else if (e.Message.StartsWith("430[null,{\"state"))
                {
                    string content = e.Message.Substring(e.Message.IndexOf("{"));
                    content = content.Substring(0, content.LastIndexOf("}"));
                    ECLogin tmplogin = json.JsonDeserialize<ECLogin>(content);
                    if (tmplogin != null)
                    {
                        username = tmplogin.username;
                        Stats.Balance = (tmplogin.balance_satoshis) / 100000000m;
                    }
                }
                else if (e.Message.StartsWith("42[\"game_tick\""))
                {
                    if (guid != "" || cashedout)
                    {
                        //42["game_tick",13969]
                        string x = e.Message.Substring(e.Message.IndexOf(",") + 1);
                        x = x.Substring(0, x.LastIndexOf("]"));
                        decimal tickval = 0;
                        if (decimal.TryParse(x, out tickval))
                        {
                            double mult = 0;
                            mult = Math.Floor(100 * Math.Pow(Math.E, 0.00006 * (double)tickval));
                            string message = "";

                            if (guid != null)
                            {
                                message = string.Format("Game running - {0:0.00000000} at {1:0.0000} - {2:0.00}x", LastBet.TotalAmount, LastBet.Payout, mult / 100);
                            }
                            else
                            {
                                message = string.Format("Game running - Cashed out - {2:0.00}x", LastBet.TotalAmount, LastBet.Payout, mult / 100);
                            }

                            callNotify(message);
                        }
                    }
                }
            }
        }
        int reqid = 1;
        string guid = "";
        private void Sock_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Logger.DumpLog(e.Exception.ToString(), -1);
            callError("Websocket error - disconnected.",true, ErrorType.Unknown );
        }

        private void Sock_Opened(object sender, EventArgs e)
        {
            (sender as WebSocket).Send("2probe");
        }

        public Task PlaceCrashBet(PlaceCrashBet BetDetails)
        {
            throw new NotImplementedException();
        }

        public class ECLogin
        {
            public string state { get; set; }
            public int game_id { get; set; }
            public string last_hash { get; set; }
            public double max_win { get; set; }
            public int elapsed { get; set; }
            public string created { get; set; }

            public string username { get; set; }
            public long balance_satoshis { get; set; }
        }
    }
}
