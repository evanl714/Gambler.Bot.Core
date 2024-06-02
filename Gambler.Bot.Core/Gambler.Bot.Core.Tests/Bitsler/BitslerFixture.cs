using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.BitslerT
{
    public class BitslerFixture : baseSiteFixture
    {
        public BitslerFixture() : base()
        {
            site = new Bitsler(logger);
        }
    }
}
