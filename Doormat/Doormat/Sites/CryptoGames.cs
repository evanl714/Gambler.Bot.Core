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
    public class CryptoGames : BaseSite, iDice
    {
        string accesstoken = "";

        public bool iscg = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public static string[] sCurrencies = new string[] { "BTC", "Doge", "ETH", "DASH", "GAS", "Bch", "STRAT", "PPC", "PLAY", "LTC", "XMR", "ETC" };
        string CurrenyHash = "";
        public CryptoGames()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", false, true, false, false) };
            this.MaxRoll = 99.999m;
            this.SiteAbbreviation = "CG";
            this.SiteName = "CryptoGames";
            this.SiteURL = "https://www.crypto-games.net?i=KaSwpL1Bky";
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
            this.CanVerify = true;
            this.Currencies = sCurrencies;
            SupportedGames = new Games.Games[] { Games.Games.Dice, Games.Games.Plinko, Games.Games.Roulette };
            this.Currency = 0;
            this.DiceBetURL = "https://www.crypto-games.net/fair.aspx?coin=BTC&type=3&id={0}";
            this.Edge = 0.8m;
        }


        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            iscg = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://api.crypto-games.net/v1/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {
                accesstoken = LoginParams[0].Value;

                string sEmitResponse = Client.GetStringAsync("user/" + CurrentCurrency + "/" + accesstoken).Result;
                cgUser tmpBal = json.JsonDeserialize<cgUser>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("nextseed/" + CurrentCurrency + "/" + accesstoken).Result;
                cgNextSeed tmpSeed = json.JsonDeserialize<cgNextSeed>(sEmitResponse);
                CurrenyHash = tmpSeed.NextServerSeedHash;
                Stats.Balance = tmpBal.Balance;
                Stats.Wagered = tmpBal.Wagered;
                Stats.Profit = tmpBal.Profit;
                Stats.Bets = tmpBal.TotalBets;
                //Get stats
                //assign vals to stats
                
                Thread t = new Thread(GetBalanceThread);
                iscg = true;
                t.Start();

                callLoginFinished(true);
            }
            catch (AggregateException e)
            {
                Logger.DumpLog(e.ToString(), -1);
                callLoginFinished(false);
            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), -1);
                callLoginFinished(false);
            }
        }
        void GetBalanceThread()
        {
            try
            {
                while (iscg)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        UpdateStats();
                    }
                    Thread.Sleep(1000);
                }
            }
            catch { }
        }
        protected override void _UpdateStats()
        {
            try
            {

                string sEmitResponse = Client.GetStringAsync("user/" + CurrentCurrency + "/" + accesstoken).Result;
                cgUser tmpBal = json.JsonDeserialize<cgUser>(sEmitResponse);
                Stats.Balance = tmpBal.Balance;
                Stats.Wagered = tmpBal.Wagered;
                Stats.Profit = tmpBal.Profit;
                Stats.Bets = tmpBal.TotalBets;
                
            }
            catch { }
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            
            string Clients = R.Next(0, int.MaxValue).ToString();
            decimal payout = decimal.Parse(((100m - Edge) / (decimal)BetDetails.Chance).ToString("0.0000"));
            cgPlaceBet tmpPlaceBet = new cgPlaceBet() { Bet = BetDetails.Amount, ClientSeed = Clients, UnderOver = BetDetails.High, Payout = (decimal)payout };

            string post = json.JsonSerializer<cgPlaceBet>(tmpPlaceBet);
            HttpContent cont = new StringContent(post);
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            /*List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("value", post));
            //pairs.Add(new KeyValuePair<string, string>("affiliate", "seuntjie"));
            FormUrlEncodedContent cont = new FormUrlEncodedContent(pairs);*/

            try
            {

                string sEmitResponse = Client.PostAsync("placebet/" + CurrentCurrency + "/" + accesstoken, cont).Result.Content.ReadAsStringAsync().Result;
                cgGetBet Response = json.JsonDeserialize<cgGetBet>(sEmitResponse);
                if (Response.Message != "" && Response.Message != null)
                {
                    callError(Response.Message,true, ErrorType.Unknown );
                    return;
                }
                DiceBet bet = new DiceBet()
                {
                    Guid = BetDetails.GUID,
                    TotalAmount = (decimal)BetDetails.Amount,
                    Profit = (decimal)Response.Profit,
                    Roll = (decimal)Response.Roll,
                    Chance = decimal.Parse(Response.Target.Substring(3), System.Globalization.NumberFormatInfo.InvariantInfo),
                    DateValue = DateTime.Now,
                    ClientSeed = Clients,
                    Currency = CurrentCurrency,
                    BetID = Response.BetId.ToString(),
                    High = Response.Target.Contains(">"),
                    ServerHash = CurrenyHash,
                    Nonce = -1,
                    ServerSeed = Response.ServerSeed
                };
                if (bet.High)
                    bet.Chance = (decimal)MaxRoll - bet.Chance;
                this.CurrenyHash = Response.NextServerSeedHash;
                bool Win = (((bool)bet.High ? (decimal)bet.Roll > (decimal)MaxRoll - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
                if (Win)
                    Stats.Wins++;
                else
                    Stats.Losses++;
                Stats.Bets++;
                Stats.Wagered += bet.TotalAmount;
                Stats.Balance += Response.Profit;
                Stats.Profit += Response.Profit;
                callBetFinished(bet);

            }
            catch
            { }
        }


        public class cgBalance
        {
            public decimal Balance { get; set; }
        }
        public class cgPlaceBet
        {
            public decimal Bet { get; set; }
            public decimal Payout { get; set; }
            public bool UnderOver { get; set; }
            public string ClientSeed { get; set; }
        }
        public class cgGetBet
        {
            public long BetId { get; set; }
            public decimal Roll { get; set; }
            public string ClientSeed { get; set; }
            public string Target { get; set; }
            public decimal Profit { get; set; }
            public string NextServerSeedHash { get; set; }
            public string ServerSeed { get; set; }
            public string Message { get; set; }
        }
        public class cgUser
        {
            public string Nickname { get; set; }
            public decimal Balance { get; set; }
            public string Coin { get; set; }
            public int TotalBets { get; set; }
            public decimal Profit { get; set; }
            public decimal Wagered { get; set; }
        }
        public class cgNextSeed
        {
            public string NextServerSeedHash { get; set; }
        }


    }
}
