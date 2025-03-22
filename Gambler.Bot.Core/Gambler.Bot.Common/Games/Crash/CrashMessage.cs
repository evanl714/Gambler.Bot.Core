using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Common.Games.Crash
{
    public class CrashMessage : IGameMessage
    {
        public Games Game { get; }

        public string Message { get; }
        public decimal Value { get; }
        public CrashMessageType MessageType { get; }

        public CrashMessage(string message, CrashMessageType MessageType, decimal value)
        {
            Game = Games.Crash;
            Message = message;
            Value = value;
        }
    }

    public enum CrashMessageType
    {
        Starting, Started, Crash,Cashout,Tick
    }
}
