using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.BitslerT
{
    public class BitslerDiceTests:DiceTests, IClassFixture<BitslerFixture>
    {
        public BitslerDiceTests(BitslerFixture site):base(site.site)
        {
            
        }
    }
}
