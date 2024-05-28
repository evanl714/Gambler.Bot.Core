using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.BitvestT
{
    public class BitvestFixture : baseSiteFixture
    {
        public BitvestFixture() : base()
        {
            site = new Bitvest(logger);
        }
    }
}
