using Gambler.Bot.Core.Enums;
using System;

namespace Gambler.Bot.Core.Events
{
    public class ErrorEventArgs : EventArgs
    {
        public bool Fatal { get; set; }
        public ErrorType Type { get; set; }
        public string Message { get; set; }
        public bool Handled { get; set; }
    }
}
