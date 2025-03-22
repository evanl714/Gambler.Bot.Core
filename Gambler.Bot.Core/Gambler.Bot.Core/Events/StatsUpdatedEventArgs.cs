using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Sites.Classes;
using System;

namespace Gambler.Bot.Core.Events
{
    public class StatsUpdatedEventArgs : EventArgs
    {
        public SiteStats NewStats { get; set; }
        public StatsUpdatedEventArgs(SiteStats Stats)
        {
            this.NewStats = Stats;
        }
    }
}
