using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.PrimeDiceT
{
    [TestCaseOrderer("Gambler.Bot.Core.Tests.Code.AlphabeticalOrderer", "Gambler.Bot.Core.Tests")]
    public class PrimeDiceTests : BaseSiteTests, IClassFixture<PrimeDiceFixture>
    {
        public PrimeDiceTests(PrimeDiceFixture fixure) : base(fixure.site)
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
            string server = "035b8940188140dcbbd39ddd9644978a5a208b73b435ae653cf1fde6e3445c3a";
            string client = "yguityuityuiy";
            int nonce = 2705;
            decimal roll = 26.24m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Dice);

            Assert.NotNull(result);
            Assert.NotNull((result as DiceResult));
            Assert.Equal(roll, (result as DiceResult).Roll);
        }

    }
}
