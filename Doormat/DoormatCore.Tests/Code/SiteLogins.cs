using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.Code
{
    public class SiteLogins
    {
        public string site { get; set; }
        public string test { get; set; }
        public LoginParamValue[] loginParams { get; set; }
    }
}
