using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.WinDiceT
{
    public class WolfbetDiceTests:DiceTests, IClassFixture<WindiceFixture>
    {
        public WolfbetDiceTests(WindiceFixture site):base(site.site)
        {
            
        }
    }
}
