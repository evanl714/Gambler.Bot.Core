using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.DuckDiceT
{
    public class DuckDiceFixture : baseSiteFixture
    {
        public DuckDiceFixture() : base()
        {
            site = new DuckDice(logger);
        }
    }
}
