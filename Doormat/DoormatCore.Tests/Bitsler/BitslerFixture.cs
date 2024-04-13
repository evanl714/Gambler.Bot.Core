using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.BitslerT
{
    public class BitslerFixture : baseSiteFixture
    {
        public BitslerFixture() : base()
        {
            site = new Bitsler(logger);
        }
    }
}
