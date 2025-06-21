using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Core.Tests.CryptoGamesT
{
    [TestCaseOrderer("Gambler.Bot.Core.Tests.Code.AlphabeticalOrderer", "Gambler.Bot.Core.Tests")]
    public class CryptoGamesTests : BaseSiteTests, IClassFixture<CryptoGamesFixture>
    {
        public CryptoGamesTests(CryptoGamesFixture fixure) : base(fixure.site)
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
            string server = "HCJGZXS4OqQ7prp9UqrmUMQbCsT4uVx3J7licEKi";
            string client = "PXfWSKXv6nzxKCnhI8n3RcwTs7L3WF2K";
            int nonce = 1;
            decimal roll = 24.895m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Dice);

            Assert.NotNull(result);
            Assert.NotNull((result as DiceResult));
            Assert.Equal(roll, (result as DiceResult).Roll);
        }
    }
}
