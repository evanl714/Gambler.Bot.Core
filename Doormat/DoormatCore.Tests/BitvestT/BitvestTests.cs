using DoormatCore.Sites;
using DoormatCore.Tests.CryptoGamesT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.BitvestT
{
    public class BitvestTests : BaseSiteTests, IClassFixture<CryptoGamesFixture>
    {
        public BitvestTests(CryptoGamesFixture fixure) : base(fixure.site)
        {

        }

    }
}
