using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.WolfBetT
{
    public class WolfbetDiceTests:DiceTests, IClassFixture<wbFixture>
    {
        public WolfbetDiceTests(wbFixture site):base(site.site)
        {
            
        }
    }
}
