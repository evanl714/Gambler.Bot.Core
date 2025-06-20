using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.StakeT
{
    [TestCaseOrderer("Gambler.Bot.Core.Tests.Code.AlphabeticalOrderer", "Gambler.Bot.Core.Tests")]
    public class StakeTests : BaseSiteTests, IClassFixture<StakeFixture>
    {
        public StakeTests(StakeFixture fixure) : base(fixure.site)
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
            string server = "69d6d96b10203b729f98590898534f0b9a9329058ba56faaea3e3af74c3a466e";
            string client = "ZgmMVEwg64";
            int nonce = 5931;
            decimal roll = 26.64m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Dice);

            Assert.NotNull(result);
            Assert.NotNull((result as DiceResult));
            Assert.Equal(roll, (result as DiceResult).Roll);
        }

        //Validate the roll verifier for Limbo
        [Fact]
        public void ValidateLimboBet()
        {
            string server = "20d7b3571337ddf17e67d77ce9d56b6677ce8713e1d3ec842f205bbb4107af60";
            string client = "621705";
            int nonce = 1;
            decimal roll = 11.30m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Limbo);

            Assert.NotNull(result);
            Assert.NotNull((result as LimboResult));
            Assert.Equal(roll, (result as LimboResult).Result);
        }
    }
}
