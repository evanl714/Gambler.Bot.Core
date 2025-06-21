using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.CryptoGamesT
{
    public class CryptoGamesDiceTests:DiceTests, IClassFixture<CryptoGamesFixture>
    {
        public CryptoGamesDiceTests(CryptoGamesFixture site):base(site.site)
        {
            
        }
    }
}
