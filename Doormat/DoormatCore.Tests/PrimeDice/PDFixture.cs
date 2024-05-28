using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.PrimeDiceT
{
    public class PrimeDiceFixture : baseSiteFixture
    {
        public PrimeDiceFixture() : base()
        {
            site = new PrimeDice(logger);
        }
    }
}
