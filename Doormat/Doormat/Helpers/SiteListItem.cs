using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore.Helpers
{
    public class SitesList
    {
        public string Name { get; set; }
        Type _SiteType = null;
        public SitesList SetType (Type NewType)
        {
            _SiteType = NewType;
            return this;
        }
        public Type SiteType()
        {
            return _SiteType;
        }
        public string[] Currencies { get; set; }
        public Games.Games[] SupportedGames { get; set; }
    }
}
