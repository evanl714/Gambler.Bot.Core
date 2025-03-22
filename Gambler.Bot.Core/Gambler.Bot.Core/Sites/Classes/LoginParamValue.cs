using System;
using System.ComponentModel;

namespace Gambler.Bot.Core.Sites.Classes
{
    public class LoginParamValue: INotifyPropertyChanged
    {

        public int ParameterId { get; set; }
        public LoginParameter Param { get; set; }
        string svalue;
        public string Value { get => svalue; set { svalue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
