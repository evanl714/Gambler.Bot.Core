using DoormatCore.Helpers;
using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using Microsoft.Extensions.Configuration;
using OtpNet;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Web;

namespace DoormatCore.Tests
{
    [TestCaseOrderer("DoormatCore.Tests.Code.AlphabeticalOrderer", "DoormatCore.Tests")]
    public abstract class BaseSiteTests
    {
        BaseSite _site;

        public BaseSiteTests(BaseSite site)
        {
            _site = site;
            _site.OnBrowserBypassRequired += _site_OnBrowserBypassRequired;
        }

        internal static LoginParamValue[] GetParams(string Sitename, [CallerMemberName] string callerName = "")
        {
            var config = new ConfigurationBuilder()
            .AddUserSecrets<BaseSiteTests>()
            .Build();
            string file = config["DICETESTACCOUNTS"];
            if (file==null)
            {
                file = Environment.GetEnvironmentVariable("DICETESTACCOUNTS");
            }
            if (File.Exists(file))
            {
                string loginthings = File.ReadAllText(file);
                SiteLogins[] logins = JsonSerializer.Deserialize<SiteLogins[]>(loginthings);
                LoginParamValue[] values = logins.FirstOrDefault(x => x.site.ToLower() == Sitename.ToLower() && x.test.ToLower() == callerName.ToLower())?.loginParams;
                if (values==null)
                {
                    Assert.True(false, $"No login details found for {Sitename} in {file}");
                }
                foreach (var x in values)
                {
                    if (x.Param.Name.ToLower()=="2fa code")
                    {
                        string tmpvalue = HttpUtility.UrlDecode(x.Value);
                        Totp totp=null;
                        if (tmpvalue.StartsWith("otpauth-migration"))
                        {
                            tmpvalue = tmpvalue.Substring(tmpvalue.IndexOf("?data=") + "?data=".Length);
                            byte[] data = Convert.FromBase64String(tmpvalue);
                            MigrationPayload tmp = MigrationPayload.Parser.ParseFrom(data);
                            //tmpvalue = tmp.OtpParameters[0].ToString();//extract secret property here
                            foreach (var otp in tmp.OtpParameters)
                            {
                                totp = new Totp(otp.Secret.ToByteArray());
                                break;
                            }
                        }
                        else
                        {
                            totp = new Totp(Base32Encoding.ToBytes(tmpvalue));
                        }
                        
                        
                        x.Value = totp.ComputeTotp(DateTime.UtcNow);
                        break;
                    }
                }
                return values;
            }
            return null;
        }

        //Tests:
        /*
         log in:
        without 2fa when not required
        with 2fa when not required
        without 2fa when required
        with 2fa when required
        valid login details
        invalid login details
        
        disconnect

        Get stats

        reset seed
        set client seed
        invest
        donate
        withdraw
        tip
        bank
        getseed
        timetobet
         

        GetLucky x 5 for each game
         */
        [STAThread]
        private void _site_OnBrowserBypassRequired(object? sender, BypassRequiredArgs e)
        {
            BrowserBypass tmp = new BrowserBypass(e.URL);
            tmp.Show();
            while (!tmp.loaded)
            {
                Thread.Sleep(100);
            }
            tmp.nav(e.URL);
            tmp.GetBypass(e);
            tmp.Close();
        }


        [Fact]
        public virtual void a1_LogInWithout2faWhenNotRequiredShouldLogIn()
        {
            string error = null;
            bool success = false;
            SiteStats stats = null;
            bool finished = false;
            
            _site.Error += (s, e) => 
            { 
                error = e.Message; finished = true; };
            _site.LoginFinished += (s, e) => 
            { 
                success= e.Success; stats = e.Stats; finished = true; };

            DateTime start = DateTime.Now;

            _site.LogIn(GetParams(_site.SiteName));

            while (!finished && (DateTime.Now-start).TotalSeconds<90)
            {
                Thread.Sleep(100);
            }

            //Error should not be null
            Assert.True(finished);
            Assert.Null(error);
            Assert.True(success);
        }

        

        [Fact]
        public virtual void a3_LogInWith2faWhenNotRequiredShouldLogIn()
        {
            string error = null;
            bool success = false;
            SiteStats stats = null;
            bool finished = false;

            _site.Error += (s, e) =>
            {
                error = e.Message; finished = true;
            };
            _site.LoginFinished += (s, e) =>
            {
                success = e.Success; stats = e.Stats; finished = true;
            };

            DateTime start = DateTime.Now;

            _site.LogIn(GetParams(_site.SiteName));

            while (!finished && (DateTime.Now - start).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            //Error should not be null
            Assert.True(finished);
            Assert.Null(error);
            Assert.True(success);

        }

        [Fact]
        public virtual void a2_LogInWithout2faWhenRequiredShouldNotLogIn()
        {
            string error = null;
            bool success = false;
            SiteStats stats = null;
            bool finished = false;

            _site.Error += (s, e) => { error = e.Message; finished = true; };
            _site.LoginFinished += (s, e) => { success = e.Success; stats = e.Stats; finished = true; };

            DateTime start = DateTime.Now;

            _site.LogIn(GetParams(_site.SiteName));

            while (!finished && (DateTime.Now - start).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            //Error should not be null
            Assert.True(finished);
            Assert.Null(error);
            Assert.False(success);
            
        }

        [Fact]
        public virtual void a4_LogInWit2faWhenRequiredShouldLogIn()
        {
            string error = null;
            bool success = false;
            SiteStats stats = null;
            bool finished = false;

            _site.Error += (s, e) =>
            {
                error = e.Message; finished = true;
            };
            _site.LoginFinished += (s, e) =>
            {
                success = e.Success; stats = e.Stats; finished = true;
            };

            DateTime start = DateTime.Now;

            _site.LogIn(GetParams(_site.SiteName));

            while (!finished && (DateTime.Now - start).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            //Error should not be null
            Assert.True(finished);
            Assert.Null(error);
            Assert.True(success);
        }

        [Fact]
        public virtual void a0_LogInWitInvalidCredentials()
        {
            string error = null;
            bool success = false;
            SiteStats stats = null;
            bool finished = false;

            _site.Error += (s, e) =>
            {
                error = e.Message; finished = true;
            };
            _site.LoginFinished += (s, e) =>
            {
                success = e.Success; stats = e.Stats; finished = true;
            };

            DateTime start = DateTime.Now;

            _site.LogIn(GetParams(_site.SiteName));

            while (!finished && (DateTime.Now - start).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            //Error should not be null
            Assert.True(finished);
            Assert.Null(error);
            Assert.False(success);
        }

        [Fact]
        public void b1_GetStatsWhenLoggedInShouldWork()
        {
            bool updated = false;
            string error = null;
            SiteStats stats = null;
            _site.StatsUpdated += (s, e) => { stats=e.NewStats; updated = true; };
            _site.Error += (s, e) => { error = e.Message; };
            DateTime start = DateTime.Now;
            _site.UpdateStats();

            while (!updated && error==null && (DateTime.Now-start).TotalSeconds<30)
            {
                Thread.Sleep(100);
            }

            Assert.NotNull(stats);
            Assert.Null(error);
            Assert.True(updated);
        }

        [Fact]
        public void b2_ResetSeedIfPossible()
        {
            if (_site.CanChangeSeed)
            {
                _site.OnResetSeedFinished += (s, e) =>
                {
                    Assert.True(e.Success);
                };

                _site.ResetSeed();
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }

        [Fact]
        public void b3_SetClientSeedIfPossible()
        {
            if ( _site.CanSetClientSeed)
            {
                string newseed = _site.GenerateNewClientSeed();
                _site.OnResetSeedFinished += (s, e) =>
                {
                    Assert.True(e.Success);
                };
                
                _site.SetClientSeed(newseed);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }

        [Fact]
        public void b4_Invest()
        {
            if (_site.AutoInvest)
            {
                decimal balance = _site.Stats.Balance;
                _site.OnInvestFinished += (s, e) =>
                {
                    Assert.NotEqual(balance,_site.Stats.Balance);
                };

                _site.Invest(balance / 2);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }

        [Fact]
        public void b5_Donate()
        {
            if (_site.CanTip || _site.AutoWithdraw)
            {
                decimal balance = _site.Stats.Balance;
                _site.OnDonationFinished += (s, e) =>
                {
                    Assert.NotEqual(balance, _site.Stats.Balance);
                };

                _site.Donate(balance / 2);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }

        [Fact]
        public void b6_Withdraw()
        {
            if ( _site.AutoWithdraw)
            {
                decimal balance = _site.Stats.Balance;
                _site.OnWithdrawalFinished += (s, e) =>
                {
                    Assert.NotEqual(balance, _site.Stats.Balance);
                };

                _site.Withdraw("get address from test configuration",balance / 2);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }

        [Fact]
        public void b7_Tip()
        {
            if (_site.CanTip )
            {
                decimal balance = _site.Stats.Balance;
                _site.OnTipFinished += (s, e) =>
                {
                    Assert.NotEqual(balance, _site.Stats.Balance);
                };

                _site.SendTip("get user from test configuration", balance / 2);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }

        /*[Fact]
        public void b8_Bank()
        {
            if (_site.CanTip)
            {
                decimal balance = _site.Stats.Balance;
                _site.OnTipFinished += (s, e) =>
                {
                    Assert.NotEqual(balance, _site.Stats.Balance);
                };

                _site.SendTip("get user from test configuration", balance / 2);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }*/
        [Fact]
        public void b9_GetSeed()
        {
            if (_site.CanGetSeed)
            {
                decimal balance = _site.Stats.Balance;
                

                string serverseed = _site.GetSeed(/*get bet id from test config*/1);
                Assert.True(false);
            }
            else
            {
                //Test is not applicable
                Assert.True(true);
            }
        }
    }
}