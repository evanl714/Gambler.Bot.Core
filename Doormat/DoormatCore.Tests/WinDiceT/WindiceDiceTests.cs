using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.WinDiceT
{
    public class WolfbetDiceTests:DiceTests, IClassFixture<WinDice>
    {
        public WolfbetDiceTests(WinDice site):base(site)
        {
            
        }
    }
}
