using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.CryptoGamesT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.BitvestT
{
    public class BitvestTests : BaseSiteTests, IClassFixture<CryptoGamesFixture>
    {
        public BitvestTests(CryptoGamesFixture fixure) : base(fixure.site)
        {

        }

    }
}
