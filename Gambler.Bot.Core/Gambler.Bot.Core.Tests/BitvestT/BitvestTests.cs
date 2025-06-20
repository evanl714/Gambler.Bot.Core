using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.CryptoGamesT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.BitvestT
{
    public class BitvestTests : BaseSiteTests, IClassFixture<BitvestFixture>
    {
        public BitvestTests(BitvestFixture fixure) : base(fixure.site)
        {

        }
        //Validate the roll verifier for dice
        [Fact]
        public void ValidateDiceBet()
        {
            string server = "d11fd6b0780e97927611ed0741c7de9032a6b9a6921be75435d26f352553fe0b";
            string client = "23d2a89515a8771c60f02e4cc6d2cf4309957dc7155cd80032731c27a";
            int nonce = 1;
            decimal roll = 35.1417m;

            var result = this._site.GetLucky(server, client, nonce, Gambler.Bot.Common.Games.Games.Dice);

            Assert.NotNull(result);
            Assert.NotNull((result as DiceResult));
            Assert.Equal(roll, (result as DiceResult).Roll);
        }
    }
}
