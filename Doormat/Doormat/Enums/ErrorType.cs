using System;

namespace Gambler.Bot.Core.Enums
{
    public enum ErrorType
    {
        InvalidBet,
        BalanceTooLow,
        BetTooLow,
        ResetSeed,
        Withdrawal,
        Tip,
        NotImplemented,
        Other,
        BetMismatch,
        Unknown
    }
}
