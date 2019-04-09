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
    class NitroDice : BaseSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        Random r = new Random();
        public static string[] sCurrencies = new string[] { "Bch", "Btc", "Doge" };

        string lastHash = "";
        public NitroDice()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Username", false, true, false, false), new LoginParameter("Password", true, true, false, true), new LoginParameter("2FA Code", false, false, true, true, true) };
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "ND";
            this.SiteName = "NitroDice";
            this.SiteURL = "http://www.nitrodice.com/?ref=EEqWBD442qC2oxjpmA1g";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = true;
            this.CanChangeSeed = false;
            this.CanChat = false;
            this.CanGetSeed = true;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = false;
            this.CanVerify = true;
            this.Currencies = sCurrencies;
            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://NitroDice.com/bets/{0}";
            this.Edge = 1;
        }


        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            ispd = false;
        }
        void GetBalanceThread()
        {
            
                while (ispd)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    {
                    UpdateStats();
                    }
                }
            
            
            }
        protected override void _Login(LoginParamValue[] LoginParams)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.nitrodice.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
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

            try
            {
                string jsoncontent = json.JsonSerializer<NDAuth>(new NDAuth() { pass = Password, user = Username, tfa = otp });
                StringContent Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                string Response = Client.PostAsync("api/auth", Content).Result.Content.ReadAsStringAsync().Result;
                NDGetAuth getauth = json.JsonDeserialize<NDGetAuth>(Response);
                if (getauth != null)
                {
                    if (getauth.token != null)
                    {
                        Client.DefaultRequestHeaders.Add("x-token", getauth.token);
                        Client.DefaultRequestHeaders.Add("x-user", Username);
                        accesstoken = getauth.token;
                        
                        string sEmitResponse2 = Client.GetStringAsync("api/stats").Result;
                        NDGetBalance tmpu = json.JsonDeserialize<NDGetBalance>(sEmitResponse2);
                        try
                        {
                            sEmitResponse2 = Client.GetStringAsync("sshash").Result;
                            NDGetHash tmpHash = json.JsonDeserialize<NDGetHash>(sEmitResponse2);
                            lastHash = tmpHash.sshash;
                        }
                        catch (Exception e)
                        {

                        }
                        Stats.Balance = tmpu.balance;
                        Stats.Profit = tmpu.amountLost + tmpu.amountWon;
                        Stats.Wins = (int)tmpu.totWins;
                        Stats.Losses = (int)tmpu.totLosses;
                        Stats.Bets = (int)tmpu.totBets;
                       
                        lastupdate = DateTime.Now;
                        ispd = true;
                        Thread t = new Thread(GetBalanceThread);
                        t.Start();
                        callLoginFinished(true);
                        return;
                    }
                }

            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
                callError(e.ToString(),true, ErrorType.Other);

            }
            callLoginFinished(false);

        }

        protected override void _UpdateStats()
        {
            try
            {
                ForceUpdateStats = false;
                lastupdate = DateTime.Now;
                string sEmitResponse2 = Client.GetStringAsync("api/stats").Result;
                NDGetBalance tmpu = json.JsonDeserialize<NDGetBalance>(sEmitResponse2);
                try
                {
                    sEmitResponse2 = Client.GetStringAsync("sshash").Result;
                    NDGetHash tmpHash = json.JsonDeserialize<NDGetHash>(sEmitResponse2);
                    lastHash = tmpHash.sshash;
                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                }
                Stats.Balance = tmpu.balance;
                Stats.Profit = tmpu.amountLost + tmpu.amountWon;
                Stats.Wins = (int)tmpu.totWins;
                Stats.Losses = (int)tmpu.totLosses;
                Stats.Bets = (int)tmpu.totBets;
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
            }
        }

        protected override void _PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            try
            {
                
                decimal amount = BetDetails.Amount;
                decimal chance = BetDetails.Chance;
                bool High = BetDetails.High;
                string clientseed = r.Next(0, int.MaxValue).ToString();

                string jsoncontent = json.JsonSerializer<NDPlaceBet>(new NDPlaceBet()
                {
                    amount = amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo),
                    perc = chance.ToString("0.0000", System.Globalization.NumberFormatInfo.InvariantInfo),
                    pos = High ? "hi" : "lo",
                    times = 1,
                    cseed = clientseed
                });
                StringContent Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                string Response = Client.PostAsync("api/bet", Content).Result.Content.ReadAsStringAsync().Result;
                NDGetBet BetResult = json.JsonDeserialize<NDGetBet>(Response);
                if (BetResult.info == null)
                {
                    DiceBet tmp = new DiceBet
                    {
                        TotalAmount = amount,
                        DateValue = DateTime.Now,
                        Chance = chance,
                        ClientSeed = clientseed
                            ,
                        ServerHash = lastHash,
                        Guid = BetDetails.GUID,
                        High = High,
                        BetID = BetResult.n.ToString(),
                        Nonce = BetResult.index,
                        Roll = BetResult.n / 10000m,
                        ServerSeed = BetResult.sseed,
                        Profit = BetResult.amount
                    };                    

                    lastHash = BetResult.sshash;
                    Stats.Bets++;
                    bool win = (tmp.Roll > 99.99m - tmp.Chance && High) || (tmp.Roll < tmp.Chance && !High);
                    Stats.Balance = BetResult.balance;
                    Stats.Wagered += amount;
                    Stats.Profit += BetResult.amount;
                    if (win)
                    {
                        Stats.Wins++;

                    }
                    else
                    {
                        Stats.Losses++;
                    }

                    callBetFinished(tmp);
                }
                else
                {
                    callError(BetResult.info,true, ErrorType.Unknown);
                }
            }
            catch (Exception Ex)
            {
                Logger.DumpLog(Ex);
                callError(Ex.ToString(), true, ErrorType.Unknown);
            }
        }


        public class NDGetAuth
        {
            public string token { get; set; }
        }

        public class NDAuth
        {
            public string user { get; set; }
            public string pass { get; set; }
            public string tfa { get; set; }
        }

        public class NDGetBalance
        {
            public decimal balance { get; set; }
            public decimal amountWon { get; set; }
            public decimal amountLost { get; set; }
            public long totWins { get; set; }
            public long totLosses { get; set; }
            public long totBets { get; set; }

        }

        public class NDPlaceBet
        {
            public string perc { get; set; }
            public string pos { get; set; }
            public string amount { get; set; }
            public int times { get; set; }
            public string cseed { get; set; }
        }

        public class NDGetBet
        {
            public long n { get; set; }
            public string r { get; set; }
            public decimal balance { get; set; }
            public long index { get; set; }
            public string sseed { get; set; }
            public string cseed { get; set; }
            public string target { get; set; }
            public long no { get; set; }
            public decimal amount { get; set; }
            public string sshash { get; set; }
            public string info { get; set; }
        }
        public class NDGetHash
        {
            public string sshash { get; set; }
        }
        public class NDChangeCoin { public string coin { get; set; } }
    }
}
