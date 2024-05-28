using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.StakeT
{
    public class StakeFixture : baseSiteFixture
    {
        public StakeFixture() : base()
        {
            site = new Stake(logger);
        }
    }
}
