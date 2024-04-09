using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.CryptoGamesT
{
    public class CryptoGamesDiceTests:DiceTests, IClassFixture<CryptoGames>
    {
        public CryptoGamesDiceTests(CryptoGames site):base(site)
        {
            
        }
    }
}
