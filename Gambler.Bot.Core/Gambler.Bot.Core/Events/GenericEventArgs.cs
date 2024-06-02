using System;

namespace Gambler.Bot.Core.Events
{
    public class GenericEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public bool Fatal { get; set; }

    }
}
