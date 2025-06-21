using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Core.Tests.DuckDiceT
{
    public class DuckDiceTests : BaseSiteTests, IClassFixture<DuckDiceFixture>
    {
        public DuckDiceTests(DuckDiceFixture fixure) : base(fixure.site)
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
            string server = "fcb11e45ceaeab9d9e683552eb354797e6ef21527568332a9324112c6aacc133";
            string client = "wA4mXnm7DNOEzqR13sZOx0Vd4FfwrV";
            int nonce = 2;
            decimal roll = 95.45m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Dice);

            Assert.NotNull(result);
            Assert.NotNull((result as DiceResult));
            Assert.Equal(roll, (result as DiceResult).Roll);
        }
    }
}
