using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Tests.Code;
using System;
using System.Linq;

namespace Gambler.Bot.Core.Tests.CryptoGamesT
{
    public class CryptoGamesFixture : baseSiteFixture
    {
        public CryptoGamesFixture() : base()
        {
            site = new CryptoGames(logger);
        }
    }
}
