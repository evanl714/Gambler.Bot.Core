using Gambler.Bot.Core.Helpers;
using System;

namespace Gambler.Bot.Core.Events
{
    public class BypassRequiredArgs : EventArgs
    {
        public string URL { get; set; }
        public BrowserConfig Config { get; set; }
        public string[] RequiredCookies { get; set; }
        public bool HasTimeout { get; set; }
        public string HeadersPath { get; set; }
    }
}
