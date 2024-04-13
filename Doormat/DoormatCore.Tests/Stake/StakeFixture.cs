using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.StakeT
{
    public class StakeFixture : baseSiteFixture
    {
        public StakeFixture() : base()
        {
            site = new Stake(logger);
        }
    }
}
