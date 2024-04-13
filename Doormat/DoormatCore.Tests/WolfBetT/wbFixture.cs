using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DoormatCore.Tests.WolfBetT
{
    public class wbFixture: baseSiteFixture
    {
        public wbFixture(): base()
        {            
            site = new WolfBet(logger);
        }
    }
}
