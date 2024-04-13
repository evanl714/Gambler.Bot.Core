using DoormatCore.Sites;
using DoormatCore.Tests.Code;
using System;
using System.Linq;

namespace DoormatCore.Tests.CryptoGamesT
{
    public class CryptoGamesFixture : baseSiteFixture
    {
        public CryptoGamesFixture() : base()
        {
            site = new CryptoGames(logger);
        }
    }
}
