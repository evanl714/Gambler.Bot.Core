using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using Gambler.Bot.Core.Tests.CryptoGamesT;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.BitvestT
{
    public class BitvestDiceTests:DiceTests, IClassFixture<BitvestFixture>
    {
        public BitvestDiceTests(BitvestFixture site):base(site.site)
        {
            
        }

       
    }
}
