using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.DuckDiceT
{
    public class DuckDiceTests : BaseSiteTests, IClassFixture<DuckDice>
    {
        public DuckDiceTests(DuckDice fixure) : base(fixure)
        {

        }
    }
}
