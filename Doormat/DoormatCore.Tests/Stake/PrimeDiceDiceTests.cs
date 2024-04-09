using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.StakeT
{
    public class StakeDiceTests:DiceTests, IClassFixture<Stake>
    {
        public StakeDiceTests(Stake site):base(site)
        {
            
        }
    }
}
