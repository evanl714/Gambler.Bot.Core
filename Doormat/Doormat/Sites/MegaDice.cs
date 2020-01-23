using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;

namespace DoormatCore.Sites
{
    class MegaDice : BaseSite, iDice
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        bool IsSatDice = false;
        DateTime LastBalance = DateTime.Now;
        string curHash = "";
        long GameID = 0;

        public MegaDice()
        {

        }
        void GetBalanceThread()
        {
            while (IsSatDice)
            {
                try
                {
                    if (((DateTime.Now - LastBalance).TotalSeconds > 15 || ForceUpdateStats) && accesstoken != "")
                    {
                        UpdateStats();
                    }
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(500);
            }
        }
        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            IsSatDice = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            string email="", Password = "" ;
            foreach (LoginParamValue x in LoginParams)
            {
                switch(x.Param.Name.ToLower() )
                {
                    case "email":
                        email = x.Value;                        
                        break;
                    case "password":
                        Password = x.Value;
                        break;
                    
                }
            }
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip};
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://session.megadice.com/userapi/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("email", email));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("login.php", Content).Result.Content.ReadAsStringAsync().Result;
                SatDiceLogin tmplog = json.JsonDeserialize<SatDiceLogin>(sEmitResponse);
                if (tmplog.message.ToLower() == "login successful.")
                {
                    accesstoken = tmplog.ctoken;
                    sEmitResponse = Client.GetStringAsync("useraddress/?ctoken=" + accesstoken).Result;
                    SatDepAddress tmpDep = json.JsonDeserialize<SatDepAddress>(sEmitResponse);
                    sEmitResponse = Client.GetStringAsync("userbalance/?ctoken=" + accesstoken).Result;
                    SatBalance tmpbal = json.JsonDeserialize<SatBalance>(sEmitResponse);
                    sEmitResponse = Client.GetStringAsync("startround/?ctoken=" + accesstoken).Result;
                    SatGameRound tmpgame = json.JsonDeserialize<SatGameRound>(sEmitResponse);
                    curHash = tmpgame.hash;
                    GameID = tmpgame.id;
                    if (GameID > 0 && curHash != "")
                    {
                        Stats.Balance = ((decimal)tmpbal.balanceInSatoshis / 100000000m);
                        
                        IsSatDice = true;
                        new System.Threading.Thread(new System.Threading.ThreadStart(GetBalanceThread)).Start();
                        callLoginFinished(true);
                        return;
                    }
                }
                else
                {
                    callLoginFinished(false);
                    return;
                }
            }
            catch { }
            callLoginFinished(true);
        }

        protected override void _UpdateStats()
        {
            LastBalance = DateTime.Now;
            string sEmitResponse = Client.GetStringAsync("userbalance/?ctoken=" + accesstoken).Result;
            SatBalance tmpbal = json.JsonDeserialize<SatBalance>(sEmitResponse);
            Stats.Balance = tmpbal.balanceInSatoshis / 100000000.0m;            
            LastBalance = DateTime.Now;
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                
                string sEmitResponse = Client.GetStringAsync(string.Format(
                    "placebet.php?ctoken={0}&betInSatoshis={1}&" +
                    "id={2}&serverHash={3}&clientRoll={4}&belowRollToWin={5}",
                    accesstoken,
                    (BetDetails.Amount * 100000000m).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo),
                    GameID,
                    curHash,
                    R.Next(0, int.MaxValue).ToString(),
                    ((BetDetails.Chance / 100m) * 65535).ToString("0"))).Result;
                SatGame betresult = json.JsonDeserialize<SatGame>(sEmitResponse);
                if (betresult.status == "success")
                {
                    DiceBet tmpRes = new DiceBet()
                    {
                        TotalAmount = (decimal)betresult.bet.betInSatoshis / 100000000m,
                        DateValue = DateTime.Now,
                        Chance = decimal.Parse(betresult.bet.probability),
                        ClientSeed = betresult.clientRoll.ToString(),
                        High = false,
                        BetID = betresult.bet.betID.ToString(),
                        Nonce = -1,
                        Profit = (decimal)betresult.bet.profitInSatoshis / 100000000m,
                        ServerHash = betresult.serverHash,
                        ServerSeed = betresult.serverRoll + "-" + betresult.serverSalt,
                        Roll = decimal.Parse(betresult.bet.rollInPercent),
                        Guid = BetDetails.GUID
                    };
                    Stats.Balance = betresult.userBalanceInSatoshis / 100000000.0m;
                    Stats.Bets++;
                    if (betresult.bet.result == "loss")
                        Stats.Losses++;
                    else
                        Stats.Wins++;
                    Stats.Wagered += tmpRes.TotalAmount;
                    Stats.Profit += tmpRes.Profit;
                    curHash = betresult.nextRound.hash;
                    GameID = betresult.nextRound.id;
                    callBetFinished(tmpRes);
                }
                else
                {
                    callError(betresult.message,false, ErrorType.Unknown);
                }
            }
            catch
            {
                callError("An error has occurred.", false, ErrorType.Unknown);
            }
        }



        public class SatDiceLogin
        {
            public string ctoken { get; set; }
            public string message { get; set; }
            public string secret { get; set; }
            public string status { get; set; }
        }

        public class SatDepAddress
        {
            public string nick { get; set; }
            public string depositaddress { get; set; }
            public double queryTimeInSeconds { get; set; }
        }
        public class SatBalance
        {

            public string nick { get; set; }
            public long balanceInSatoshis { get; set; }
            public string unconfirmedBalanceInsSatoshis { get; set; }
            public string hash { get; set; }
            public long maxProfitInSatoshis { get; set; }
            public double queryTimeInSeconds { get; set; }
        }

        public class SatGameRound
        {
            public long id { get; set; }
            public string hash { get; set; }
            public string welcomeMessage { get; set; }
            public long maxProfitInSatoshis { get; set; }

        }

        public class SatBet
        {
            public string game { get; set; }
            public long betID { get; set; }
            public string betTX { get; set; }
            public string playerNick { get; set; }
            public string playerHash { get; set; }
            public string betType { get; set; }
            public long target { get; set; }
            public string probability { get; set; }
            public int streak { get; set; }
            public int roll { get; set; }
            public string rollInPercent { get; set; }
            public string time { get; set; }
            public string result { get; set; }
            public long betInSatoshis { get; set; }
            public string prize { get; set; }
            public long payoutInSatoshis { get; set; }
            public string payoutTX { get; set; }
            public long profitInSatoshis { get; set; }
        }
        public class SatGame
        {
            public double newLuck { get; set; }
            public string message { get; set; }
            public string serverRoll { get; set; }
            public string serverSalt { get; set; }
            public string serverHash { get; set; }
            //public string clientRoll { get; set; }
            public int resultingRoll { get; set; }
            public int clientRoll { get; set; }
            public long userBalanceInSatoshis { get; set; }
            public SatGameRound nextRound { get; set; }
            public string status { get; set; }
            public SatBet bet { get; set; }
        }
        public class SatWithdraw
        {
            public double amountWithdrawn { get; set; }
            public string transactionId { get; set; }
            public int status { get; set; }
            public string message { get; set; }
            public int confirmationsRequired { get; set; }
            public double balance { get; set; }
        }
    }
}
