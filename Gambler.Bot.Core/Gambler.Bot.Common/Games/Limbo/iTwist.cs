using System;
using System.Linq;

namespace Gambler.Bot.Common.Games.Limbo
{
    /*
     Twist is the same as the limbo but I want to maintain the names of the games as they are at the casinos.
     */
    public interface iTwist
    {
        public Task<LimboBet> PlaceLimboBet(PlaceLimboBet bet);
    }
}
