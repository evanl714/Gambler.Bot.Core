using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Sites.Classes;
using System;

namespace Gambler.Bot.Core.Events
{
    public class LoginFinishedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public SiteStats Stats { get; set; }
        public LoginFinishedEventArgs(bool Success, SiteStats Stats)
        {
            this.Success = Success;
            this.Stats = Stats;
        }
    }
}
