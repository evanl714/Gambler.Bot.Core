using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.WolfBetT
{
    [TestCaseOrderer("Gambler.Bot.Core.Tests.Code.AlphabeticalOrderer", "Gambler.Bot.Core.Tests")]
    public class WolfbetTests : BaseSiteTests, IClassFixture<wbFixture>
    {
        public WolfbetTests(wbFixture fixure) : base(fixure.site)
        {

        }

        [Fact]
        public override void a2_LogInWithout2faWhenRequiredShouldNotLogIn()
        {
            //test s not applicable
            Assert.True(true);
        }
        [Fact]
        public override void a3_LogInWith2faWhenNotRequiredShouldLogIn()
        {
            //test s not applicable
            Assert.True(true);
        }
        [Fact]
        public override void a4_LogInWit2faWhenRequiredShouldLogIn()
        {
            //test s not applicable
            Assert.True(true);
        }

        //Validate the roll verifier for dice
        [Fact]
        public void ValidateDiceBet()
        {
            string server = "c56666ed3e6d69854fd2c5aa72377456b3f49253f25a8ddab8f935cacf0f3717";
            string client = "b1da34d7006ccd399a58290287a715a1";
            int nonce = 5173;
            decimal roll = 87.48m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Dice);

            Assert.NotNull(result);
            Assert.NotNull((result as DiceResult));
            Assert.Equal(roll, (result as DiceResult).Roll);
        }

        //Validate the roll verifier for Limbo
        [Fact]
        public void ValidateLimboBet()
        {
            string server = "504f0640e546bb72d58bedb82dbe74a71ca3f53083fdf6ad7560eaa9ec65eabc";
            string client = "b1da34d7006ccd399a58290287a715a1";
            int nonce = 1;
            decimal roll = 1.53m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Limbo);

            Assert.NotNull(result);
            Assert.NotNull((result as LimboResult));
            Assert.Equal(roll, (result as LimboResult).Result);
        }
    }
}
