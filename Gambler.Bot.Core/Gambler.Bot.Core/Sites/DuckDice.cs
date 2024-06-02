using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Gambler.Bot.Core.Enums;
using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites.Classes;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.Core.Sites
{
    public class DuckDice : BaseSite, iDice
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        public static string[] cCurrencies = new string[] { "BTC", "ETH", "LTC", "DOGE", "DASH", "BCH", "XMR", "XRP", "ETC", "BTG", "XLM", "ZEC" };
        QuackSeed currentseed = null;

        public DuckDice(ILogger logger) : base(logger)
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", true, true, false, true) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "DD";
            this.SiteName = "DuckDice";
            this.SiteURL = "https://duckdice.io/?c=53ea652da4";
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
            this.CanVerify = true;
            this.Currencies = cCurrencies;
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://duckdice.io/Bets/{0}";
            this.Edge = 1m;
            NonceBased = true;
        }

        void GetBalanceThread()
        {

            while (ispd)
            {
                if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                {
                    lastupdate = DateTime.Now;
                    UpdateStats();

                }
                Thread.Sleep(1000);
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

        protected override async Task<bool> _Login(LoginParamValue[] LoginParams)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            ClientHandlr.CookieContainer = new CookieContainer();
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://duckdice.io/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
            try
            {

                accesstoken = LoginParams[0].Value;

                HttpResponseMessage EmitResponse = await Client.GetAsync("https://dickdice.io");
                //if (!EmitResponse.IsSuccessStatusCode)
                {
                    var cookies = CallBypassRequired(SiteURL, "__cf_bm");

                    HttpClientHandler handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        UseCookies = true,
                        CookieContainer = cookies.Cookies,

                    };
                    Client = new HttpClient(handler) { BaseAddress = new Uri("https://duckdice.io/api/") }; ;
                    Client.DefaultRequestHeaders.Add("referrer", SiteURL);
                    Client.DefaultRequestHeaders.Add("accept", "*/*");
                    Client.DefaultRequestHeaders.Add("origin", SiteURL);
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(cookies.UserAgent);
                    Client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                    Client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                    Client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                }
               

                EmitResponse = await Client.GetAsync("load/" + CurrentCurrency + "?api_key=" + accesstoken);
                if (EmitResponse.IsSuccessStatusCode)
                {
                    string sEmitResponse = await EmitResponse.Content.ReadAsStringAsync();
                    Quackbalance balance = JsonSerializer.Deserialize<Quackbalance>(sEmitResponse);
                    sEmitResponse = await Client.GetStringAsync("stat/" + CurrentCurrency + "?api_key=" + accesstoken);
                    QuackStatsDetails _Stats = JsonSerializer.Deserialize<QuackStatsDetails>(sEmitResponse);
                    sEmitResponse = await Client.GetStringAsync("randomize" + "?api_key=" + accesstoken);
                    currentseed = JsonSerializer.Deserialize<QuackSeed>(sEmitResponse).current;
                    if (balance != null && _Stats != null)
                    {
                        Stats.Balance = decimal.Parse(balance.user.balances.main, System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Profit = decimal.Parse(_Stats.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Wagered = decimal.Parse(_Stats.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                        Stats.Bets = _Stats.bets;
                        Stats.Wins = _Stats.wins;
                        Stats.Losses = _Stats.bets - _Stats.wins;


                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(new ThreadStart(GetBalanceThread)).Start();
                        callLoginFinished(true);
                        return true;
                    }
                }
                else
                {
                    string response =await EmitResponse.Content.ReadAsStringAsync();
                    callLoginFinished(false);
                    return false;
                }
            }
            catch (Exception e)
            {
                callLoginFinished(false);
                return false;
            }
            callLoginFinished(false);
            return false;
        }

        protected override async Task<SiteStats> _UpdateStats()
        {
            try
            {

                string sEmitResponse = await Client.GetStringAsync("load/" + CurrentCurrency + "?api_key=" + accesstoken);
                Quackbalance balance = JsonSerializer.Deserialize<Quackbalance>(sEmitResponse);
                Stats.Balance = decimal.Parse(balance.user.balances.main, System.Globalization.NumberFormatInfo.InvariantInfo);
                sEmitResponse = await Client.GetStringAsync("stat/" + CurrentCurrency + "?api_key=" + accesstoken);
                QuackStatsDetails _Stats = JsonSerializer.Deserialize<QuackStatsDetails>(sEmitResponse);
                Stats.Profit = decimal.Parse(_Stats.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                Stats.Wagered = decimal.Parse(_Stats.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                Stats.Bets = _Stats.bets;
                Stats.Wins = _Stats.wins;
                Stats.Losses = _Stats.bets - _Stats.wins;
                return Stats;
            }
            catch (Exception e)
            {
                _logger?.LogError(e.ToString());
            }
            return null;
        }

        public async Task<DiceBet> PlaceDiceBet(PlaceDiceBet BetDetails)
        {

            decimal amount = BetDetails.Amount;
            decimal chance = BetDetails.Chance;
            bool High = BetDetails.High;
            StringContent Content = new StringContent(string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"amount\":\"{0:0.00000000}\",\"symbol\":\"{1}\",\"chance\":{2:0.00},\"isHigh\":{3}}}", amount, CurrentCurrency, chance, High ? "true" : "false"), Encoding.UTF8, "application/json");
            try
            {
                var response = await Client.PostAsync("play" + "?api_key=" + accesstoken, Content);
                string sEmitResponse = await response.Content.ReadAsStringAsync();
                QuackBet newbet = JsonSerializer.Deserialize<QuackBet>(sEmitResponse);
                if (newbet.error != null || newbet.errors!=null)
                {
                    ErrorType type = ErrorType.Unknown;
                    string msg = newbet.error;

                    if (newbet.error != null)
                    {
                        if (newbet.error == "You have insufficient balance.")
                            type = ErrorType.BalanceTooLow;
                        else if (newbet.error.StartsWith("The minimum bet is"))
                            type = ErrorType.BetTooLow;
                    }
                    else
                    {
                        if (newbet.errors?.chance?.FirstOrDefault(x => x.StartsWith("The chance may not be greater than")) != null)
                        {
                            type = ErrorType.InvalidBet;
                            msg = newbet.errors.chance.FirstOrDefault();

                        }
                    }
                    
                    callError(msg, false, type);
                    return null;
                }
                DiceBet tmp = new DiceBet
                {
                    TotalAmount = decimal.Parse(newbet.bet.betAmount, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Chance = newbet.bet.chance,
                    ClientSeed = currentseed.clientSeed,
                    Currency = CurrentCurrency,
                    DateValue = DateTime.Now,
                    High = High,
                    Nonce = currentseed.nonce++,
                    Profit = decimal.Parse(newbet.bet.profit, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Roll = newbet.bet.number / 100,
                    ServerHash = currentseed.serverSeedHash,
                    BetID = newbet.bet.hash,
                    Guid = BetDetails.GUID
                };
                lastupdate = DateTime.Now;
                Stats.Profit = decimal.Parse(newbet.user.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                Stats.Wagered = decimal.Parse(newbet.user.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                Stats.Balance = decimal.Parse(newbet.user.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                Stats.Wins = newbet.user.wins;
                Stats.Bets = newbet.user.bets;
                Stats.Losses = newbet.user.bets - newbet.user.wins;
                callBetFinished(tmp);
                return tmp;
            }
            catch (Exception e)
            {
                callError("There was an error placing your bet.", false, ErrorType.Unknown);
                _logger?.LogError(e.ToString());
            }
            return null;
        }

        public class QuackLogin
        {
            public string token { get; set; }
        }
        
        public class QuackErrors
        {
            public string[] chance { get; set; }
            
        }
        public class QuackStats
        {

            public QuackStats user { get; set; }
            public string hash { get; set; }
            public string username { get; set; }
            public string balance { get; set; }
            public QuackStats session { get; set; }
            public int bets { get; set; }
            public int wins { get; set; }
            public string volume { get; set; }
            public string profit { get; set; }

        }
        public class QuackStatsDetails
        {
            public int bets { get; set; }
            public int wins { get; set; }
            public string profit { get; set; }
            public string volume { get; set; }
        }
        public class QuackBet
        {
            public string error { get; set; }
            public QuackErrors errors { get; set; }
            public QuackBet bet { get; set; }
            public QuackStats user { get; set; }
            public string hash { get; set; }
            public string symbol { get; set; }
            public bool result { get; set; }
            public bool isHigh { get; set; }
            public decimal number { get; set; }
            public decimal threshold { get; set; }
            public decimal chance { get; set; }
            public decimal payout { get; set; }
            public string betAmount { get; set; }
            public string winAmount { get; set; }
            public string profit { get; set; }
            public long nonce { get; set; }

        }
        public class QuackSeed
        {
            public QuackSeed current { get; set; }
            public string clientSeed { get; set; }
            public long nonce { get; set; }
            public string serverSeedHash { get; set; }
        }
        public class QuackWithdraw
        {
            public string error { get; set; }

        }


        public class Quackbalance
        {
            public QuackUser user { get; set; }
            public QuackBet[] bets { get; set; }
        }

        public class QuackUser
        {
            public Balances balances { get; set; }
            public object[] presets { get; set; }
            public string hash { get; set; }
            public string username { get; set; }
        }

        public class Balances
        {
            public string main { get; set; }
            public string faucet { get; set; }
        }      
    

    }
}
