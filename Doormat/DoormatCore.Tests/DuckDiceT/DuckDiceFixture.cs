using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.DuckDiceT
{
    public class DuckDiceFixture : baseSiteFixture
    {
        public DuckDiceFixture() : base()
        {
            site = new DuckDice(logger);
        }
    }
}
