using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using Gambler.Bot.Core.Tests.CryptoGamesT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.BitvestT
{
    public class BitvestDiceTests:DiceTests, IClassFixture<CryptoGamesFixture>
    {
        public BitvestDiceTests(CryptoGamesFixture site):base(site.site)
        {
            
        }
    }
}
