using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DoormatCore.Games;
using DoormatCore.Helpers;

namespace DoormatCore.Sites
{
    public class YoloDice : BaseSite, iDice
    {
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        /*HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;*/
        TcpClient apiclient = new TcpClient();
        SslStream sslStream;
        long id = 0;
        string basestring = "{{\"id\":{0},\"method\":\"{1}\"{2}}}\r\n";
        public static string[] sCurrencies = new string[] { "Btc", "Ltc", "Eth" };

        public YoloDice()
        {
            StaticLoginParams = new LoginParameter[] { new LoginParameter("Private Key", true, true, false, true) };
            Currencies = sCurrencies;
            DiceBetURL = "https://yolodice.com/#{0}";
            SiteURL = "https://yolodice.com/r?fexD-GR";
            this.MaxRoll = 99.99m;
            this.SiteAbbreviation = "YD";
            this.SiteName = "YoloDice";
            this.SiteURL = "https://yolodice.com/r?fexD-GR";
            this.Stats = new SiteStats();
            this.TipUsingName = true;
            this.AutoInvest = false;
            this.AutoWithdraw = true;
            this.CanChangeSeed = true;
            this.CanChat = false;
            this.CanGetSeed = false;
            this.CanRegister = false;
            this.CanSetClientSeed = false;
            this.CanTip = false;
            this.CanVerify = false;

            SupportedGames = new Games.Games[] { Games.Games.Dice };
            this.Currency = 0;
            this.DiceBetURL = "https://bitvest.io/bet/{0}";
            this.Edge = 1;
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _Disconnect()
        {
            throw new NotImplementedException();
        }

        byte[] ReadBuffer = new byte[512];
        string challenge = "";
        string privkey = "";

        protected override void _Login(LoginParamValue[] LoginParams)
        {
            try
            {
                privkey = LoginParams[0].Value;
                apiclient = new TcpClient();

                apiclient.Connect("api.yolodice.com", 4444);
                if (apiclient.Connected)
                {
                    sslStream = new SslStream(apiclient.GetStream());
                    sslStream.AuthenticateAsClient("api.yolodice.com");//, null, System.Security.Authentication.SslProtocols.Ssl2| System.Security.Authentication.SslProtocols.Ssl3| System.Security.Authentication.SslProtocols.Tls11|System.Security.Authentication.SslProtocols.Tls12, false);

                    string frstchallenge = string.Format(basestring, id++, "generate_auth_challenge", "");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    int bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLChallenge tmp = null;
                    try
                    {
                        tmp = json.JsonDeserialize<YLChallenge>(challenge);
                    }
                    catch (Exception e)
                    {
                        callError("error: ",true, ErrorType.Other);
                        callLoginFinished(false);
                        return;
                    }
                    string address = "";
                    string message = "";
                    try
                    {
                        NBitcoin.Key tmpkey = NBitcoin.Key.Parse(privkey, NBitcoin.Network.Main);
                        address = tmpkey.ScriptPubKey.GetDestinationAddress(NBitcoin.Network.GetNetwork("Main")).ToString();
                        message = tmpkey.SignMessage(tmp.result);
                    }
                    catch (Exception e)
                    {
                        callError("API key format error. Are you using your Private key?",true, ErrorType.Other);
                        callLoginFinished(false);
                        return;
                    }
                    frstchallenge = string.Format(basestring, id++, "auth_by_address", ",\"params\":" + json.JsonSerializer<YLAuthSend>(new YLAuthSend { address = address, signature = message }));
                    
                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLOgin tmologin = null;
                    try
                    {
                        tmologin = json.JsonDeserialize<YLOgin>(challenge);
                    }
                    catch (Exception e)
                    {
                        callError("error: " + challenge,true, ErrorType.Other);
                        callLoginFinished(false);
                        return;
                    }

                    uid = tmologin.result.id;
                    this.username = tmologin.result.name;
                    frstchallenge = string.Format(basestring, id++, "read_user_coin_data", ",\"params\":{\"selector\":{\"id\":\"" + uid + "_" + CurrentCurrency.ToLower() + "\"}}");
                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLUserStats tmpstats = null;
                    try
                    {
                        tmpstats = json.JsonDeserialize<YLUserStats>(challenge).result;
                    }
                    catch (Exception e)
                    {
                        callError("error: " + challenge,true, ErrorType.Other);
                        callLoginFinished(false);
                        return;
                    }

                    if (tmpstats != null)
                    {

                        Stats.Balance = (decimal)tmpstats.balance / 100000000m;
                        Stats.Bets = (int)tmpstats.bets;
                        Stats.Wins = (int)tmpstats.wins;
                        Stats.Losses = (int)tmpstats.losses;
                        Stats.Profit = (decimal)tmpstats.profit / 100000000m;
                        Stats.Wagered = (decimal)tmpstats.wagered / 100000000m;
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(new ThreadStart(BalanceThread)).Start();
                        
                        new Thread(new ThreadStart(Beginreadthread)).Start();
                        Thread.Sleep(50);
                        callLoginFinished(true);

                        return;
                    }
                    //tmp2.ImportPrivKey(Password, "", false);
                    //string message = tmp2.SignMessage(username, tmp.result);
                    //string message = //FullSignatureTest(tmp.result, new AsymmetricCipherKeyPair("", ""));


                    /*ispd = true;
                    new Thread(new ThreadStart(BalanceThread)).Start();*/
                }

            }
            catch (Exception e)
            {
                Logger.DumpLog(e);

            }
            callLoginFinished(false);
        }

        protected override void _UpdateStats()
        {
            ForceUpdateStats = false;
            lastupdate = DateTime.Now;
            Write("read_user_coin_data", "{\"selector\":{\"id\":\"" + uid + "_" + CurrentCurrency.ToLower() + "\"}}");
        }

        void BalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 15 || ForceUpdateStats)
                {
                    UpdateStats();

                }
                Thread.Sleep(1000);
            }
        }
        void ReadTCP(IAsyncResult result)
        {
            try
            {
                try
                {
                    string response = "";

                    response = Encoding.ASCII.GetString(ReadBuffer, 0, 512);

                    response = response.Replace("\0", "");
                    Logger.DumpLog(response, 6);
                    if (response != "")
                    {

                        try
                        {
                            response = response.Substring(0, response.IndexOf("\n"));

                            YLBasicResponse tmprespo = json.JsonDeserialize<YLBasicResponse>(response);
                            if (Requests.ContainsKey(tmprespo.id))
                            {
                                switch (Requests[tmprespo.id])
                                {
                                    case "read_user_coin_data": ReadUser(response); break;
                                    case "create_bet": ProcessBet(response); break;
                                    case "read_current_seed": read_current_seed(response); break;
                                    case "read_seed": read_current_seed(response); break;
                                    case "create_seed": create_seed(response); break;
                                    case "patch_seed": patch_seed(response); break;
                                    case "create_withdrawal": create_withdrawal(response); break;
                                    case "ping": ping(response); break;

                                }
                                Requests.Remove(tmprespo.id);
                            }
                        }
                        catch (Exception e)
                        {
                           callError("Error: " + response,true, ErrorType.Unknown);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!apiclient.Connected)
                    {
                        if (!inauth)
                            Auth();
                    }
                }
                ReadBuffer = new byte[512];
                if (apiclient.Connected)
                {
                    try
                    {
                        sslStream.EndRead(result);
                        sslStream.BeginRead(ReadBuffer, 0, 512, ReadTCP, sslStream);
                    }
                    catch
                    {
                        if (!apiclient.Connected)
                        {
                            if (!inauth)
                                Auth();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!apiclient.Connected)
                {
                    if (!inauth)
                        Auth();
                }
            }

        }
        void Beginreadthread()
        {
            sslStream.BeginRead(ReadBuffer, 0, 512, ReadTCP, sslStream);
            Write("read_current_seed", "{\"selector\":{\"user_id\":" + uid + "}}");

        }
        string Guid = "";
        
        
        void ProcessBet(string response)
        {
            YLBetResponse tmpbetrespo = json.JsonDeserialize<YLBetResponse>(response).result;
            lastbet = DateTime.Now;
            delay = tmpbetrespo.delay;
            if (tmpbetrespo != null)
            {
                DiceBet tmp = new DiceBet()
                {
                    Guid = Guid,
                    BetID = tmpbetrespo.id.ToString(),
                    TotalAmount = (decimal)tmpbetrespo.amount / 100000000m,
                    DateValue = DateTime.Now,
                    Chance = (decimal)tmpbetrespo.target / 10000m,
                    High = tmpbetrespo.range == "hi",
                    Profit = (decimal)tmpbetrespo.profit / 100000000m,
                    Roll = (decimal)tmpbetrespo.rolled / 10000m,
                    Nonce = tmpbetrespo.nonce,
                    Currency = tmpbetrespo.coin
                };
                bool sent = false;
                DateTime StartWait = DateTime.Now;
                if (Currentseed == null)
                {
                    //while (Currentseed == null && (DateTime.Now-StartWait).TotalSeconds<20)
                    {
                        if (!sent)
                        {
                            sent = true;
                            Write("read_seed", "{\"selector\":{\"id\":" + tmpbetrespo.seed_id + "}}");
                            callNotify("Getting seed data. Please wait.");
                        }
                        //Thread.Sleep(100);
                    }
                }
                if (Currentseed != null)
                {
                    if (Currentseed.id != tmpbetrespo.seed_id)
                    {
                        //while (Currentseed.id != tmpbetrespo.seed_id && (DateTime.Now - StartWait).TotalSeconds < 20)
                        {
                            if (!sent)
                            {
                                sent = true;
                                Write("read_seed", "{\"selector\":{\"id\":" + tmpbetrespo.seed_id + "}}");
                                callNotify("Getting seed data. Please wait.");
                            }
                            //Thread.Sleep(100);
                        }
                    }
                }
                if (Currentseed != null)
                {

                    tmp.ServerHash = Currentseed.secret_hashed;
                    tmp.ClientSeed = Currentseed.client_seed;
                }
                if (tmpbetrespo.user_data != null)
                {
                    Stats.Balance = (decimal)tmpbetrespo.user_data.balance / 100000000m;
                    Stats.Bets = (int)tmpbetrespo.user_data.bets;
                    Stats.Wins = (int)tmpbetrespo.user_data.wins;
                    Stats.Losses = (int)tmpbetrespo.user_data.losses;
                    Stats.Profit = (decimal)tmpbetrespo.user_data.profit / 100000000m;
                    Stats.Wagered = (decimal)tmpbetrespo.user_data.wagered / 100000000m;
                }
                else
                {
                    Stats.Balance += tmp.Profit;
                    Stats.Wagered += tmp.TotalAmount;
                    Stats.Bets++;
                    bool win = false;
                    if ((tmp.Roll > MaxRoll - tmp.Chance && tmp.High) || (tmp.Roll < tmp.Chance && !tmp.High))
                    {
                        win = true;
                    }
                    if (win)
                        Stats.Wins++;
                    else
                        Stats.Losses++;
                }
                callBetFinished(tmp);
            }
        }
        YLSeed Currentseed = null;
        void read_current_seed(string response)
        {
            YLSeed tmp = json.JsonDeserialize<YLSeed>(response).result;
            if (tmp != null)
            {
                Currentseed = tmp;
            }
        }
        void create_seed(string response)
        {
            YLSeed tmp = json.JsonDeserialize<YLSeed>(response).result;
            if (tmp != null)
            {
                Currentseed = tmp;
            }
        }
        void patch_seed(string response)
        {

        }
        void create_withdrawal(string response)
        {

            Write("read_user_coin_data", "{\"selector\":{\"id\":\"" + uid + "_" + CurrentCurrency.ToLower() + "\"}}");

        }
        void ping(string response)
        {

        }
        void ReadUser(string response)
        {
            YLUserStats tmpstats = json.JsonDeserialize<YLUserStats>(response).result;
            if (tmpstats != null)
            {
                Stats.Balance = (decimal)tmpstats.balance / 100000000m;
                Stats.Bets = (int)tmpstats.bets;
                Stats.Wins = (int)tmpstats.wins;
                Stats.Losses = (int)tmpstats.losses;
                Stats.Profit = (decimal)tmpstats.profit / 100000000m;
                Stats.Wagered = (decimal)tmpstats.wagered / 100000000m;                
            }
        }
        DateTime lastbet = DateTime.Now;
        decimal delay = 0;

        Dictionary<long, string> Requests = new Dictionary<long, string>();
        void Write(string Method, string Params)
        {
            if (apiclient.Connected)
            {
                try
                {
                    string s = string.Format(basestring, id, Method, (Params == "" ? "" : ",\"params\":" + Params));

                    byte[] bytes = Encoding.UTF8.GetBytes(s);

                    Requests.Add(id++, Method);
                    sslStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception e)
                {
                    if (apiclient.Connected)
                       callError("It seems an error has occured!",true, ErrorType.Unknown);
                    else
                    {
                        if (ispd)
                        {
                            callError("Disconnected. Reconnecting... Click start in a few seconds.", true, ErrorType.Unknown);
                            if (!inauth)
                                Auth();
                        }
                    }

                }
            }
            else if (ispd && !inauth)
            {
                Auth();
            }
        }
        bool inauth = false;
        void Auth()
        {
            inauth = true;
            try
            {
                apiclient.Close();

            }
            catch { }
            try
            {
                apiclient = new TcpClient();

                apiclient.Connect("api.yolodice.com", 4444);
                if (apiclient.Connected)
                {
                    sslStream = new SslStream(apiclient.GetStream());
                    sslStream.AuthenticateAsClient("https://api.yolodice.com");

                    string frstchallenge = string.Format(basestring, id++, "generate_auth_challenge", "");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    int bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLChallenge tmp = null;
                    try
                    {
                        tmp = json.JsonDeserialize<YLChallenge>(challenge);
                    }
                    catch (Exception e)
                    {
                        Logger.DumpLog(e);
                        callError("error: " + challenge,true, ErrorType.Unknown);
                        /*finishedlogin(false);
                        return;*/
                    }
                    NBitcoin.Key tmpkey = NBitcoin.Key.Parse(privkey, NBitcoin.Network.Main);
                    string address = tmpkey.ScriptPubKey.GetDestinationAddress(NBitcoin.Network.GetNetwork("Main")).ToString();
                    string message = tmpkey.SignMessage(tmp.result);

                    frstchallenge = string.Format(basestring, id++, "auth_by_address", ",\"params\":" + json.JsonSerializer<YLAuthSend>(new YLAuthSend { address = address, signature = message }));

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLOgin tmologin = null;
                    try
                    {
                        tmologin = json.JsonDeserialize<YLOgin>(challenge);
                    }
                    catch (Exception e)
                    {
                        Logger.DumpLog(e);
                        callError("error: " + challenge,true, ErrorType.Unknown);
                        /*finishedlogin(false);
                        return;*/
                    }

                    uid = tmologin.result.id;
                    this.username = tmologin.result.name;
                    frstchallenge = string.Format(basestring, id++, "read_user_coin_data", ",\"params\":{\"selector\":{\"id\":\"" + uid + "_" + CurrentCurrency.ToLower() + "\"}}");
                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLUserStats tmpstats = null;
                    try
                    {
                        tmpstats = json.JsonDeserialize<YLUserStats>(challenge).result;
                    }
                    catch (Exception e)
                    {
                       callError("error: " + challenge,true, ErrorType.Unknown);
                        /*finishedlogin(false);
                        return;*/
                    }

                    if (tmpstats != null)
                    {

                        Stats.Balance = (decimal)tmpstats.balance / 100000000m;
                        Stats.Bets = (int)tmpstats.bets;
                        Stats.Wins = (int)tmpstats.wins;
                        Stats.Losses = (int)tmpstats.losses;
                        Stats.Profit = (decimal)tmpstats.profit / 100000000m;
                        Stats.Wagered = (decimal)tmpstats.wagered / 100000000m;
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(new ThreadStart(BalanceThread)).Start();
                        new Thread(new ThreadStart(Beginreadthread)).Start();
                        
                        inauth = false;

                        return;
                    }                    
                }
            }
            catch (Exception e)
            {

            }
            //finishedlogin(false);
            inauth = false;

            
        }


        public void PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            this.Guid = BetDetails.GUID;

            string bet = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"attrs\":{0}}}",
                json.JsonSerializer<YLBetSend>(
                    new YLBetSend { amount = (long)(BetDetails.Amount * 100000000), range = BetDetails.High ? "hi" : "lo", target = (int)(BetDetails.Chance * 10000), coin = CurrentCurrency.ToLower() }));
            Write("create_bet", bet);
        }


        public class YLAuthSend
        {
            public string address { get; set; }
            public string signature { get; set; }
        }
        public class YLBasicResponse
        {
            public int id { get; set; }
        }
        public class YLChallenge
        {
            public int id { get; set; }
            public string result { get; set; }
        }
        public class YLOgin
        {
            public long id { get; set; }
            public YLOgin result { get; set; }
            public string name { get; set; }
        }
        public class YLUserStats
        {
            public string id { get; set; }
            //public long user_id { get; set; }
            public long bets { get; set; }
            public long wins { get; set; }
            public long losses { get; set; }
            public long profit { get; set; }
            public long wagered { get; set; }
            public long profit_min { get; set; }
            public long profit_max { get; set; }
            public decimal luck { get; set; }
            //public long chat_message_count  { get; set; }
            public long balance { get; set; }
            public YLUserStats result { get; set; }
        }
        public class YLBetResponse
        {
            public string coin { get; set; }
            public long id { get; set; }
            public YLBetResponse result { get; set; }
            public YLOgin user { get; set; }
            public string created_at { get; set; }
            public long target { get; set; }
            public string range { get; set; }
            public long amount { get; set; }
            public long rolled { get; set; }
            public long profit { get; set; }
            public long seed_id { get; set; }
            public long nonce { get; set; }
            public decimal delay { get; set; }
            public YLUserStats user_data { get; set; }

        }
        public class YLBetSend
        {
            public long amount { get; set; }
            public string range { get; set; }
            public int target { get; set; }
            public string coin { get; set; }
        }
        public class YLSeed
        {
            public long id { get; set; }
            public long user_id { get; set; }
            public string created_at { get; set; }
            public long bet_count { get; set; }
            public string client_seed { get; set; }
            public bool current { get; set; }
            public string secret_hashed { get; set; }
            public string secret { get; set; }
            public YLSeed result { get; set; }
        }
        public class YLWithdrawal
        {
            public string to_address { get; set; }
            public long amount { get; set; }
            public bool allow_pending { get; set; }
            public string coin { get; set; }
        }
        public class YLUserCoin
        {

        }
    }
}
