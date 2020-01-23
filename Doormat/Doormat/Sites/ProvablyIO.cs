using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;

namespace DoormatCore.Sites
{
    class ProvablyIO : BaseSite, iDice
    {
        string lasthash = "";
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool iskd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public string LastHash { get; set; }
        RandomNumberGenerator Rg = new System.Security.Cryptography.RNGCryptoServiceProvider();

        public ProvablyIO()
        {

        }
        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {
                    if ((DateTime.Now - lastupdate).TotalSeconds > 30 || ForceUpdateStats)
                    {
                        lastupdate = DateTime.Now;
                        
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
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://coinpro.fit/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
            Client.DefaultRequestHeaders.Add("Host", "coinpro.fit");
            Client.DefaultRequestHeaders.Add("Origin", "https://coinpro.fit/");
            Client.DefaultRequestHeaders.Add("Referer", "https://coinpro.fit/");
            accesstoken = LoginParams[0].Value;
            
            try
            {
                //ClientHandlr.CookieContainer.Add(new Cookie("socket", Password,"/","coinpro.fit"));
                ClientHandlr.CookieContainer.Add(new Cookie("PHPSESSID", accesstoken, "/", "coinpro.fit"));
                //string page = Client.GetStringAsync()
                string Stats = Client.GetStringAsync("userstats").Result;
                PIOStats tmpstats = json.JsonDeserialize<PIOStats>(Stats);
                
                this.Stats.Balance = (tmpstats.user.balances.btc) / 100000000m;
                this.Stats.Bets = (int)tmpstats.user.stats.btc.bets;
                this.Stats.Wagered = (tmpstats.user.stats.btc.wagered) / 100000000m;
                this.Stats.Profit = (tmpstats.user.stats.btc.profit) / 100000000m;
                
                iskd = true;
                lastupdate = DateTime.Now;
                //lasthash=tmpstats.user.
                new Thread(new ThreadStart(GetBalanceThread)).Start();
                callLoginFinished(true);
            }
            catch
            {
                callLoginFinished(false);
                return;
            }
        }

        protected override void _UpdateStats()
        {
            string Stats = Client.GetStringAsync("userstats").Result;
            PIOStats tmpstats = json.JsonDeserialize<PIOStats>(Stats);
            this.Stats.Balance = ((decimal)tmpstats.user.balances.btc) / 100000000m;
            this.Stats.Bets = (int)tmpstats.user.stats.btc.bets;
            this.Stats.Wagered = ((decimal)tmpstats.user.stats.btc.wagered) / 100000000m;
            this.Stats.Profit = ((decimal)tmpstats.user.stats.btc.profit) / 100000000m;
            
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                
                byte[] bytes = new byte[4];
                Rg.GetBytes(bytes);
                string seed = ((long)BitConverter.ToUInt32(bytes, 0)).ToString();
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("wager", (BetDetails.Amount).ToString("0.00000000")));
                pairs.Add(new KeyValuePair<string, string>("region", BetDetails.High ? ">" : "<"));
                pairs.Add(new KeyValuePair<string, string>("target", (BetDetails.High ? MaxRoll - BetDetails.Chance : BetDetails.Chance).ToString("0.00")));
                pairs.Add(new KeyValuePair<string, string>("odds", BetDetails.Chance.ToString("0.00")));
                pairs.Add(new KeyValuePair<string, string>("clientSeed", seed));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("bet", Content).Result.Content.ReadAsStringAsync().Result;
                CoinProBet tmpbet = json.JsonDeserialize<CoinProBet>(sEmitResponse);
                DiceBet tmp = new DiceBet
                {
                    Guid = BetDetails.GUID,
                    TotalAmount = (decimal)BetDetails.Amount,
                    DateValue = DateTime.Now,
                    BetID = tmpbet.bet_id.ToString(),
                    Profit = (decimal)tmpbet.profit / 100000000m,
                    Roll = (decimal)tmpbet.outcome,
                    High = BetDetails.High,
                    Chance = (decimal)BetDetails.Chance,
                    Nonce = (int)(tmpbet.outcome * 100),
                    ServerHash = lasthash,
                    ServerSeed = tmpbet.secret.ToString(),
                    ClientSeed = seed
                };

                lasthash = tmpbet.next_hash;
                Stats.Bets++;
                bool Win = (((bool)tmp.High ? (decimal)tmp.Roll > (decimal)MaxRoll - (decimal)(tmp.Chance) : (decimal)tmp.Roll < (decimal)(tmp.Chance)));
                if (Win)
                    Stats.Wins++;
                else
                    Stats.Losses++;
                Stats.Wagered += BetDetails.Amount;
                Stats.Profit += tmp.Profit;
                Stats.Balance = tmpbet.balance / 100000000m;
                callBetFinished(tmp);
            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), -1);
                callError("An unknown error has occured while placing a bet.",false, ErrorType.Unknown);
            }
        }



        public class PIOBet
        {
            public long id { get; set; }
            public long bet_id { get; set; }
            public long secret { get; set; }
            public long balance { get; set; }
            public long high { get; set; }
            public decimal outcome { get; set; }
            public decimal profit { get; set; }
            public string salt { get; set; }
            public string created_at { get; set; }
            public string next_hash { get; set; }
            public string error { get; set; }

        }
        public class PIOStats
        {
            public int id { get; set; }
            public int app_id { get; set; }
            public PIOUser user { get; set; }
        }
        public class PIOUser
        {
            public string uname { get; set; }
            public string role { get; set; }
            public PIOBalances balances { get; set; }
            public PIOUnconfirmed unconfirmed { get; set; }
            public PIOStats2 stats { get; set; }
        }
        public class PIOBalances
        {
            public decimal btc { get; set; }
            public decimal ltc { get; set; }
            public decimal dash { get; set; }
        }

        public class PIOUnconfirmed
        {
            public decimal btc { get; set; }
            public decimal ltc { get; set; }
            public decimal dash { get; set; }
        }

        public class PIOBtc
        {
            public decimal wagered { get; set; }
            public decimal bets { get; set; }
            public decimal profit { get; set; }
        }

        public class PIOLtc
        {
            public decimal wagered { get; set; }
            public decimal bets { get; set; }
            public decimal profit { get; set; }
        }

        public class PIODash
        {
            public decimal wagered { get; set; }
            public decimal bets { get; set; }
            public decimal profit { get; set; }
        }

        public class PIOStats2
        {
            public PIOBtc btc { get; set; }
            public PIOLtc ltc { get; set; }
            public PIODash dash { get; set; }
        }


        public class CoinProBet
        {
            public string id { get; set; }
            public string bet_id { get; set; }
            public decimal outcome { get; set; }
            public decimal profit { get; set; }
            public string secret { get; set; }
            public string salt { get; set; }
            public string created_at { get; set; }
            public string next_hash { get; set; }
            public long raw_outcome { get; set; }
            public decimal balance { get; set; }
            public decimal high { get; set; }
            public string betRow { get; set; }
            public string error { get; set; }
        }
    }
}
