using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.PrimeDiceT
{
    public class PrimeDiceFixture : baseSiteFixture
    {
        public PrimeDiceFixture() : base()
        {
            site = new PrimeDice(logger);
        }
    }
}
