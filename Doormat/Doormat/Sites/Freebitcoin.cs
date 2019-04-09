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
    public class Freebitcoin : BaseSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;


        public Freebitcoin()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            this.MaxRoll = 100m;
            this.SiteAbbreviation = "FBtc";
            this.SiteName = "FreeBitcoin";
            this.SiteURL = "https://freebitco.in/?r=2310118";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = false;
            this.CanChangeSeed = false;
            this.CanChat = false;
            this.CanGetSeed = true;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = true;
            this.CanVerify = true;
            Currencies = new string[] {"Btc" };
            
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://freebitco.in/?r=2310118&bet={0}";
            this.Edge = 5m;
        }


        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            ispd = false;
        }
        CookieContainer Cookies = new CookieContainer();
        string csrf = "";
        string address = "";
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
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://freebitco.in/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            ClientHandlr.CookieContainer = Cookies;
            try
            {
                string s1 = "";
                HttpResponseMessage resp = Client.GetAsync("").Result;
                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                        //cflevel = 0;
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            callNotify("freebitcoin has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        /*if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "freebitco.in"))
                        {

                            finishedlogin(false);
                            return;
                        }*/

                    }
                }
                foreach (Cookie x in Cookies.GetCookies(new Uri("https://freebitco.in")))
                {
                    if (x.Name == "csrf_token")
                    {
                        csrf = x.Value;
                    }
                }
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("csrf_token", csrf));
                pairs.Add(new KeyValuePair<string, string>("op", "login_new"));
                pairs.Add(new KeyValuePair<string, string>("btc_address", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("tfa_code", otp));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                var EmitResponse = Client.PostAsync("" + accesstoken, Content).Result;

                if (EmitResponse.IsSuccessStatusCode)
                {
                    string s = EmitResponse.Content.ReadAsStringAsync().Result;
                    string[] messages = s.Split(':');
                    if (messages.Length > 2)
                    {
                        address = messages[1];
                        accesstoken = messages[2];
                        Cookies.Add(new Cookie("btc_address", address, "/", "freebitco.in"));
                        Cookies.Add(new Cookie("password", accesstoken, "/", "freebitco.in"));
                        Cookies.Add(new Cookie("have_account", "1", "/", "freebitco.in"));

                        s = Client.GetStringAsync("https://freebitco.in/cgi-bin/api.pl?op=get_user_stats").Result;
                        FreebtcStats stats = json.JsonDeserialize<FreebtcStats>(s);
                        if (stats != null)
                        {
                            Stats.Balance = stats.balance / 100000000m;
                            Stats.Bets = (int)stats.rolls_played;
                            Stats.Wins = Stats.Losses = 0;
                            Stats.Profit = stats.dice_profit / 100000000m;
                            Stats.Wagered = stats.wagered / 100000000m;
                           
                            lastupdate = DateTime.Now;
                            ispd = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            callLoginFinished(true);
                            return;
                        }
                        callLoginFinished(false);
                        return;

                    }
                    callLoginFinished(false);
                    return;
                }


                //Lastbet = DateTime.Now;

            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), 1);
            }
            callLoginFinished(false);
            return;
        }

        protected override void _UpdateStats()
        {
            try
            {
                lastupdate = DateTime.Now;
                string s = Client.GetStringAsync("https://freebitco.in/cgi-bin/api.pl?op=get_user_stats").Result;
                FreebtcStats stats = json.JsonDeserialize<FreebtcStats>(s);
                if (stats != null)
                {
                    Stats.Balance = stats.balance / 100000000m;
                    Stats.Bets = (int)stats.rolls_played;
                    //wins = losses = 0;
                    Stats.Profit = stats.dice_profit / 100000000m;
                    Stats.Wagered = stats.wagered / 100000000m;
                    

                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
            }
        }


        void GetBalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 30)
                {
                    lastupdate = DateTime.Now;
                    UpdateStats();
                }
            }
        }
        string clientseed = "";

        protected override void _PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
               
                bool High = BetDetails.High;
                decimal amount = BetDetails.Amount;
                decimal chance = BetDetails.Chance;
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm1234567890";
                while (clientseed.Length < 16)
                {
                    clientseed += chars[R.Next(0, chars.Length)];
                }
                string Params = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "m={0}&client_seed={1}&jackpot=0&stake={2}&multiplier={3}&rand={5}&csrf_token={4}",
                    High ? "hi" : "lo", clientseed, amount, (100m - Edge) / chance, csrf, R.Next(0, 9999999) / 10000000);

                var betresult = Client.GetAsync("https://freebitco.in/cgi-bin/bet.pl?" + Params).Result;
                if (betresult.IsSuccessStatusCode)
                {

                    string Result = betresult.Content.ReadAsStringAsync().Result;
                    string[] msgs = Result.Split(':');
                    if (msgs.Length > 2)
                    {

                        /*
                            1. Success code (s1)
                            2. Result (w/l)
                            3. Rolled number
                            4. User balance
                            5. Amount won or lost (always positive). If 2. is l, then amount is subtracted from balance else if w it is added.
                            6. Redundant (can ignore)
                            7. Server seed hash for next roll
                            8. Client seed of previous roll
                            9. Nonce for next roll
                            10. Server seed for previous roll
                            11. Server seed hash for previous roll
                            12. Client seed again (can ignore)
                            13. Previous nonce
                            14. Jackpot result (1 if won 0 if not won)
                            15. Redundant (can ignore)
                            16. Jackpot amount won (0 if lost)
                            17. Bonus account balance after bet
                            18. Bonus account wager remaining
                            19. Max. amount of bonus eligible
                            20. Max bet
                            21. Account balance before bet
                            22. Account balance after bet
                            23. Bonus account balance before bet
                            24. Bonus account balance after bet
                         */
                        DiceBet tmp = new DiceBet
                        {
                            Guid = BetDetails.GUID,
                            TotalAmount = amount,
                            DateValue = DateTime.Now,
                            Chance = chance,
                            ClientSeed = msgs[7],
                            High = High,
                           BetID = Stats.Bets.ToString(),
                            Profit = msgs[1] == "w" ? decimal.Parse(msgs[4]) : -decimal.Parse(msgs[4], System.Globalization.NumberFormatInfo.InvariantInfo),
                            Nonce = long.Parse(msgs[12], System.Globalization.NumberFormatInfo.InvariantInfo),
                            ServerHash = msgs[10],
                            ServerSeed = msgs[9],
                            Roll = decimal.Parse(msgs[2], System.Globalization.NumberFormatInfo.InvariantInfo) / 100.0m

                        };
                        Stats.Balance = decimal.Parse(msgs[3], System.Globalization.NumberFormatInfo.InvariantInfo);
                        if (msgs[1] == "w")
                            Stats.Wins++;
                        else Stats.Losses++;
                        Stats.Bets++;
                        Stats.Wagered += amount;
                        Stats.Profit += tmp.Profit;
                        callBetFinished(tmp);

                    }
                    else if (msgs.Length > 0)
                    {
                        //20 - too low balance
                        if (msgs.Length > 1)

                        {
                            if (msgs[1] == "20")
                            {
                                callError("Balance too low.",true, ErrorType.BalanceTooLow);
                            }
                        }
                        else
                        {
                            callError("Site returned unknown error. Retrying in 30 seconds.", true, ErrorType.Unknown);
                        }
                    }
                    else
                    {
                        callError("Site returned unknown error. Retrying in 30 seconds.",true, ErrorType.Unknown);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                callError("An internal error occured. Retrying in 30 seconds.",true, ErrorType.Unknown);
            }

        }

        public class FreebtcStats
        {
            public long wagered { get; set; }
            public long rolls_played { get; set; }
            public decimal lottery_spent { get; set; }
            public string status { get; set; }
            public decimal jackpot_winnings { get; set; }
            public decimal jackpot_spent { get; set; }
            public decimal reward_points { get; set; }
            public decimal balance { get; set; }
            public decimal total_winnings { get; set; }
            public decimal dice_profit { get; set; }
        }
    }
}
