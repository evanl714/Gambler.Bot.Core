using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.BitslerT
{
    public class BitslerDiceTests:DiceTests, IClassFixture<Bitsler>
    {
        public BitslerDiceTests(Bitsler site):base(site)
        {
            
        }
    }
}
