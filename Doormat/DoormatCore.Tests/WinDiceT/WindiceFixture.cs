using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.WinDiceT
{
    public class WindiceFixture : baseSiteFixture
    {
        public WindiceFixture() : base()
        {
            site = new WinDice(logger);
        }
    }
}
