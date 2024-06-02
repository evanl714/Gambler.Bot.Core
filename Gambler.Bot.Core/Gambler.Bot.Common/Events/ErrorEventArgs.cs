using Gambler.Bot.Common.Enums;
using System;

namespace Gambler.Bot.Common.Events
{
    public class ErrorEventArgs : EventArgs
    {
        public bool Fatal { get; set; }
        public ErrorType Type { get; set; }
        public string Message { get; set; }
        public bool Handled { get; set; }
    }
}
