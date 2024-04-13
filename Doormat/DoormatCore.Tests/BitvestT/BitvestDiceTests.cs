using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using DoormatCore.Tests.CryptoGamesT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.BitvestT
{
    public class BitvestDiceTests:DiceTests, IClassFixture<CryptoGamesFixture>
    {
        public BitvestDiceTests(CryptoGamesFixture site):base(site.site)
        {
            
        }
    }
}
