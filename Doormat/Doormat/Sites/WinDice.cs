using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using Random = DoormatCore.Helpers.Random;

namespace DoormatCore.Sites
{
    public class WinDice : BaseSite, iDice
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;        
        Random R = new Random();
        WDGetSeed currentseed;

        public WinDice()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", false, true, false, false) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "WD";
            this.SiteName = "WinDice";
            this.SiteURL = "https://windice.io/?r=08406hjdd";
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
            this.Currencies = new string[] { "btc", "eth", "ltc", "doge" };
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://windice.io/api/v1/api/getBet?hash={0}";
            this.Edge = 1;
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            
            decimal low = 0;
            decimal high = 0;
            if (BetDetails.High)
            {
                high = MaxRoll * 100;
                low = (MaxRoll - BetDetails.Chance) * 100 + 1;
            }
            else
            {
                high = BetDetails.Chance * 100 - 1;
                low = 0;
            }
            string loginjson = JsonSerializer.Serialize<WDPlaceBet>(new WDPlaceBet()
            {
                curr = CurrentCurrency,
                bet = BetDetails.Amount,
                game = "in",
                high = (int)high,
                low = (int)low
            });

            HttpContent cont = new StringContent(loginjson);
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage resp2 = Client.PostAsync("roll", cont).Result;

            if (resp2.IsSuccessStatusCode)
            {
                string response = resp2.Content.ReadAsStringAsync().Result;
                WDBet tmpBalance = JsonSerializer.Deserialize<WDBet>(response);
                if (tmpBalance.status == "success")
                {
                    DiceBet Result = new DiceBet()
                    {
                        TotalAmount = BetDetails.Amount,
                        DateValue = DateTime.Now,
                        Chance = tmpBalance.data.chance,
                        ClientSeed = currentseed.client,
                        Currency = CurrentCurrency,
                        Guid = BetDetails.GUID,
                        BetID = tmpBalance.data.hash,
                        High = BetDetails.High,
                        Nonce = tmpBalance.data.nonce,
                        Profit = tmpBalance.data.win - tmpBalance.data.bet,
                        Roll = tmpBalance.data.result / 100m,
                        ServerHash = currentseed.hash
                    };
                    Stats.Bets++;
                    bool Win = (((bool)BetDetails.High ? (decimal)Result.Roll > (decimal)MaxRoll - (decimal)(BetDetails.Chance) : (decimal)Result.Roll < (decimal)(BetDetails.Chance)));
                    if (Win)
                        Stats.Wins++;
                    else Stats.Losses++;
                    Stats.Wagered += BetDetails.Amount;
                    Stats.Profit += Result.Profit;
                    Stats.Balance += Result.Profit;
                    callBetFinished(Result);
                }
                else
                {
                    callNotify(tmpBalance.message);
                    callError(tmpBalance.message, false, ErrorType.Unknown);
                }
            }
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            ispd = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://windice.io/api/v1/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("UserAgent", "DiceBot");
            Client.DefaultRequestHeaders.Add("Authorization", LoginParams.First(m=>m.Param?.Name?.ToLower()=="api key")?.Value);
            try
            {
                if (getbalance())
                {
                    getstats();
                    getseed();
                    ispd = true;
                    lastupdate = DateTime.Now;

                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    //lasthash = tmpblogin.server_hash;
                    callLoginFinished(true);
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                callLoginFinished(false);
            }
        }
        void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if (((DateTime.Now - lastupdate).TotalSeconds > 30 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        ForceUpdateStats = false;
                        getbalance();
                        getstats();
                    }
                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                }
                Thread.Sleep(1000);
            }
        }
        bool getbalance()
        {
            string response = Client.GetStringAsync("user").Result;
            WDUserResponse tmpBalance = JsonSerializer.Deserialize<WDUserResponse>(response);
            if (tmpBalance.data != null)
            {
                PropertyInfo tmp = typeof(WDBalance).GetProperty(CurrentCurrency.ToLower());
                if (tmp != null)
                {
                    decimal balance = (decimal)tmp.GetValue(tmpBalance.data.balance);
                    Stats.Balance = balance;                    
                }
            }
            return tmpBalance.status == "success";
        }
        bool getstats()
        {
            string response = Client.GetStringAsync("stats").Result;
            WDStatsResponse tmpBalance = JsonSerializer.Deserialize<WDStatsResponse>(response);
            if (tmpBalance.data != null)
            {
                foreach (WDStatistic x in tmpBalance.data.statistics)
                {
                    if (x.curr == CurrentCurrency)
                    {
                        Stats.Wagered = x.bet;
                        Stats.Profit = x.profit;
                        
                        break;
                    }
                }
                Stats.Bets = tmpBalance.data.stats.bets;
                Stats.Wins = tmpBalance.data.stats.wins;
                Stats.Losses = tmpBalance.data.stats.loses;                
            }
            return tmpBalance.status == "success";
        }
        bool getseed()
        {
            string response = Client.GetStringAsync("seed").Result;
            WDGetSeed tmpBalance = JsonSerializer.Deserialize<WDGetSeed>(response);
            if (tmpBalance.data != null)
            {
                currentseed = tmpBalance.data;
            }
            return tmpBalance.status == "success";
        }
        protected override void _UpdateStats()
        {
            getstats();
        }


        public class WDBaseResponse
        {
            public string status { get; set; }
            public string message { get; set; }
        }

        public class WDUserResponse : WDBaseResponse
        {
            public WDUserResponse data { get; set; }
            public string hash { get; set; }
            public string username { get; set; }
            public string avatar { get; set; }
            public int rating { get; set; }
            public int reg_time { get; set; }
            public bool hide_profit { get; set; }
            public bool hide_bet { get; set; }
            public WDBalance balance { get; set; }
        }
        public class WDBalance
        {
            public decimal btc { get; set; }
            public decimal eth { get; set; }
            public decimal ltc { get; set; }
            public decimal doge { get; set; }
        }
        public class WDPlaceBet
        {
            public string curr { get; set; }
            public decimal bet { get; set; }
            public string game { get; set; }
            public int low { get; set; }
            public int high { get; set; }
        }
        public class WDBet : WDBaseResponse
        {
            public WDBet data { get; set; }
            public string hash { get; set; }
            public string userHash { get; set; }
            public string username { get; set; }
            public int nonce { get; set; }
            public string curr { get; set; }
            public decimal bet { get; set; }
            public decimal win { get; set; }
            public decimal jackpot { get; set; }
            public decimal pointLow { get; set; }
            public decimal pointHigh { get; set; }
            public string game { get; set; }
            public decimal chance { get; set; }
            public decimal payout { get; set; }
            public decimal result { get; set; }
            public decimal time { get; set; }
            public bool isHigh { get; set; }
        }
        public class WDStatistic
        {
            public string curr { get; set; }
            public decimal bet { get; set; }
            public decimal profit { get; set; }
        }

        public class WDStats
        {
            public int wins { get; set; }
            public int loses { get; set; }
            public int bets { get; set; }
            public int chat { get; set; }
            public int online { get; set; }
        }

        public class WDStatsResponse : WDBaseResponse
        {
            public WDStatsResponse data { get; set; }
            public WDStatistic[] statistics { get; set; }
            public WDStats stats { get; set; }
        }
        public class WDGetSeed : WDBaseResponse
        {
            public WDGetSeed data { get; set; }
            public string client { get; set; }
            public string hash { get; set; }
            public string newHash { get; set; }
            public int nonce { get; set; }
        }
        public class WDResetSeed
        {
            public string value { get; set; }

        }
        public class WDResetResult : WDBaseResponse
        {
            public WDResetResult data { get; set; }
            public string client { get; set; }
            public string hash { get; set; }
            public long nonce { get; set; }
        }
    }
}
