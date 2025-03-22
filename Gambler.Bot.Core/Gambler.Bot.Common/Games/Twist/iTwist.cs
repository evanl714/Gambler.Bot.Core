using Gambler.Bot.Common.Games.Limbo;
using System;

namespace Gambler.Bot.Common.Games.Dice
{
    public interface iTwist
    {
        Task<TwistBet> PlaceTwistBet(PlaceTwistBet BetDetails);
        TwistConfig TwistSettings { get; set; }

    }
}
