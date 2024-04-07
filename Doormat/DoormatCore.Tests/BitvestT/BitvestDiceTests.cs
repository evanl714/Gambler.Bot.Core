using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.BitvestT
{
    public class BitvestDiceTests:DiceTests, IClassFixture<Bitvest>
    {
        public BitvestDiceTests(Bitvest site):base(site)
        {
            
        }
    }
}
