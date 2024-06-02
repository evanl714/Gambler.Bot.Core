using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.WinDiceT
{
    public class WindiceFixture : baseSiteFixture
    {
        public WindiceFixture() : base()
        {
            site = new WinDice(logger);
        }
    }
}
