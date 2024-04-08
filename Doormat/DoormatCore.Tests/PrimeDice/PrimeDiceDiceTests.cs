using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.PrimeDiceT
{
    public class PrimeDiceDiceTests:DiceTests, IClassFixture<PrimeDice>
    {
        public PrimeDiceDiceTests(PrimeDice site):base(site)
        {
            
        }
    }
}
