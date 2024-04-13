using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.BitvestT
{
    public class BitvestFixture : baseSiteFixture
    {
        public BitvestFixture() : base()
        {
            site = new Bitvest(logger);
        }
    }
}
