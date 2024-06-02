using System;

namespace Gambler.Bot.Core.Sites.Classes
{
    public class LoginParameter
    {
        public LoginParameter(string Name, bool Masked, bool Required, bool ClearafterEnter, bool Clearafterlogin, bool ismfa = false)
        {
            this.Name = Name;
            this.Masked = Masked;
            this.Required = Required;
            ClearAfterEnter = ClearafterEnter;
            ClearAfterLogin = Clearafterlogin;
            IsMFA = IsMFA;
        }

        public LoginParameter()
        {
        }

        public string Name { get; set; }
        public bool Masked { get; set; }
        public bool Required { get; set; }
        public bool ClearAfterEnter { get; set; }
        public bool ClearAfterLogin { get; set; }
        public bool IsMFA { get; set; }
        public string PasswordChar { get => Masked ? "*" : null; }
    }
}
