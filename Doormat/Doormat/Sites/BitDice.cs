using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DoormatCore.Sites
{
    class BitDice : BaseSite, iDice
    {
        bool isbitdice = false;
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        string APIKey = "";
        BDSeed CurrentSeed;
        DateTime LastUpdate = DateTime.Now;

        public BitDice()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("API Key", false, true, false, false) };
            this.MaxRoll = 99.9999m;
            this.SiteAbbreviation = "BD";
            this.SiteName = "BitDice";
            this.SiteURL = "https://www.bitdice.me/?r=65";
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
            this.Currencies = new string[] { "btc", "doge", "ltc", "eth", "csno", "eos" };
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://www.bitdice.me/{0}";
            this.Edge = 1;
        }

        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                
                
                string Highlow = BetDetails.High ? "high" : "low";
                string request = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "dice?api_key={0}&currency={1}&amount={2}&chance={3}&type={4}",
                    APIKey,
                    CurrentCurrency,
                    BetDetails.Amount,
                    BetDetails.Chance,
                    Highlow);
                var BetResponse = Client.PostAsync(request, new StringContent("")).Result;
                string sbetresult = BetResponse.Content.ReadAsStringAsync().Result;

                BDBetResponse NewBet = json.JsonDeserialize<BDBetResponse>(sbetresult);
                try
                {
                    if (!string.IsNullOrWhiteSpace(NewBet.error))
                    {
                        if(NewBet.error.ToLower().StartsWith("minimum bet amount is") || NewBet.error.ToLower().StartsWith("you can bet on "))
                        {
                            callError(NewBet.error, false, ErrorType.InvalidBet);
                        }
                        else if (NewBet.error.ToLower()== "your balance is too low")
                        {
                            callError(NewBet.error, false, ErrorType.BalanceTooLow);
                        }
                        
                        callError(NewBet.error, false, ErrorType.Unknown);                        
                        return;
                    }
                    if (CurrentSeed.id != NewBet.bet.data.secret)
                    {
                        string SecretResponse = Client.GetStringAsync($"dice/secret?api_key={APIKey}").Result;
                        BDSeed tmpSeed = json.JsonDeserialize<BDSeed>(SecretResponse);
                        CurrentSeed = tmpSeed;
                    }
                    Bet result = new DiceBet
                    {
                        TotalAmount = NewBet.bet.amount,
                        DateValue = DateTime.Now,
                        Chance = NewBet.bet.data.chance,
                        ClientSeed = CurrentSeed.hash,//-----------THIS IS WRONG! THIS NEEDS TO BE FIXED AT SOME POINT BEFORE GOING LIVE
                        Guid = BetDetails.GUID,
                        Currency = Currencies[Currency],
                        High = NewBet.bet.data.high,
                        BetID = NewBet.bet.id.ToString(),
                        Nonce = NewBet.bet.data.nonce,
                        Profit = NewBet.bet.profit,
                        Roll = NewBet.bet.data.lucky,
                        ServerHash = CurrentSeed.hash,
                        //serverseed = NewBet.old.secret
                    };


                    //CurrentSeed = new BDSeed { hash = NewBet.secret.hash, id = NewBet.secret.id };
                    bool win = NewBet.bet.data.result;
                    if (win)
                        Stats.Wins++;
                    else Stats.Losses++;
                    Stats.Bets++;
                    Stats.Wagered += result.TotalAmount;
                    Stats.Profit += result.Profit;
                    Stats.Balance = NewBet.balance;
                    
                    callBetFinished(result);
                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                    callError(e.ToString(), false, ErrorType.Unknown);
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                callError(e.ToString(), false, ErrorType.Unknown);
            }
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            this.isbitdice = false;
        }

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
      | SecurityProtocolType.Tls11
      | SecurityProtocolType.Tls12;
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }; ;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitdice.me/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            APIKey = LoginParams.First(m => m.Param.Name.ToLower() == "api key")?.Value;
            try
            {
                string Response = Client.GetStringAsync($"user/balance?api_key={APIKey}&currency={CurrentCurrency}").Result;
                BDSTats tmpStats = json.JsonDeserialize<BDSTats>(Response);
                //Parent.DumpLog(Response, -1);
                string SecretResponse = Client.GetStringAsync($"dice/secret?api_key={APIKey}").Result;
                BDSeed tmpSeed = json.JsonDeserialize<BDSeed>(SecretResponse);
                //Parent.DumpLog(SecretResponse, -1);
                Stats.Balance = tmpStats.balance;
                Stats.Wagered = tmpStats.wagered;
                Stats.Profit = tmpStats.profit;

                

                CurrentSeed = tmpSeed;
                LastUpdate = DateTime.Now;
                isbitdice = true;
                new Thread(new ThreadStart(GetBalanceThread)).Start();

                callLoginFinished(true);
                return;
            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), -1);
            }
            callLoginFinished(false);
        }

        void GetBalanceThread()
        {
            while (isbitdice)
            {
                if ((DateTime.Now - LastUpdate).TotalSeconds > 60 || ForceUpdateStats)
                {
                    LastUpdate = DateTime.Now;
                    UpdateStats();
                }
                Thread.Sleep(1000);
            }
        }
        protected override void _UpdateStats()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(APIKey))
                {
                    string Response = Client.GetStringAsync($"user/balance?api_key={APIKey}&currency={CurrentCurrency}").Result;
                    BDSTats tmpStats = json.JsonDeserialize<BDSTats>(Response);
                    //Parent.DumpLog(SecretResponse, -1);
                    Stats.Balance = tmpStats.balance;
                    Stats.Wagered = tmpStats.wagered;
                    Stats.Profit = tmpStats.profit;
                    
                }
            }
            catch (Exception e)
            {
                Logger.DumpLog(e.ToString(), -1);
            }
        }

        protected override decimal _GetLucky(string Hash, string ServerSeed, string ClientSeed, int Nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < ServerSeed.Length; i++)
            {
                serverb.Add(Convert.ToByte(ServerSeed[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */ClientSeed;
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }


        public class BDSTats
        {
            public decimal balance { get; set; }
            public string currency { get; set; }
            public decimal profit { get; set; }
            public decimal wagered { get; set; }
        }
        public class BDSeed
        {
            public string hash { get; set; }
            public long id { get; set; }
        }
        public class BDUser
        {
            public int level { get; set; }
            public string username { get; set; }
        }

        public class BDBetData
        {
            public decimal chance { get; set; }
            public bool high { get; set; }
            public decimal lucky { get; set; }
            public decimal multiplier { get; set; }
            public bool result { get; set; }
            public long secret { get; set; }
            public decimal target { get; set; }
            public BDUser user { get; set; }
            public long nonce { get; set; }
        }

        public class BDBet
        {
            public decimal amount { get; set; }
            public string currency { get; set; }
            public BDBetData data { get; set; }
            public long date { get; set; }
            public long game { get; set; }
            public long id { get; set; }
            public decimal profit { get; set; }
            public decimal wagered { get; set; }

        }

        public class BDJackpot
        {
            public bool status { get; set; }
        }

        public class BDOld
        {
            public string client { get; set; }
            public string hash { get; set; }
            public string secret { get; set; }
        }

        public class BDSecret
        {
            public decimal game { get; set; }
            public string hash { get; set; }
            public long id { get; set; }
        }

        public class BDBetResponse
        {
            public string error { get; set; }
            public decimal balance { get; set; }
            public BDBet bet { get; set; }
            public BDJackpot jackpot { get; set; }
            public BDOld old { get; set; }
            public BDSecret secret { get; set; }
        }


    }
}
