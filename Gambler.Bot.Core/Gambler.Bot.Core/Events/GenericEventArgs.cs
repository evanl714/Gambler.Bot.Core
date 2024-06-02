using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
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
