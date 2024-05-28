using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.PrimeDiceT
{
    public class PrimeDiceDiceTests:DiceTests, IClassFixture<PrimeDiceFixture>
    {
        public PrimeDiceDiceTests(PrimeDiceFixture site):base(site.site)
        {
            
        }
    }
}
