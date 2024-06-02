using Gambler.Bot.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Common.Interfaces
{
    public interface IProvablyFair
    {
        decimal GetLucky(string ServerSeed, string ClientSeed, int Nonce);
        string GetHash(string ServerSeed);
        string GenerateNewClientSeed();
        GRandom Random { get; }
    }
}
