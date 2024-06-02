using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.StakeT
{
    public class StakeDiceTests:DiceTests, IClassFixture<StakeFixture>
    {
        public StakeDiceTests(StakeFixture site):base(site.site)
        {
            
        }
    }
}
