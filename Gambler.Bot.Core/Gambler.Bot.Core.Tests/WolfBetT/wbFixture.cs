using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.WolfBetT
{
    public class wbFixture: baseSiteFixture
    {
        public wbFixture(): base()
        {            
            site = new WolfBet(logger);
        }
    }
}
