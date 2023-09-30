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
        public string Img { get {return  $@"Assets\Images\Sites\{Name}.png"; } }
        public string[] Currencies { get; set; } = new string[0];
        public Games.Games[] SupportedGames { get; set; } = new Games.Games[0];
        public string URL { get; set; }

        string gamesString = "";
        public string GamesString { 
            get 
            {
                if (string.IsNullOrWhiteSpace(gamesString))
                {
                    gamesString = "";
                    foreach (Games.Games x in SupportedGames?? new Games.Games[0])
                    {
                        if (gamesString != "")
                            gamesString += ", ";
                        gamesString += x.ToString();
                    }
                }
                return gamesString;
            } 
        }
        string currencyString = "";
        public string CurrencyString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(currencyString))
                {
                    currencyString = "";
                    foreach (string x in Currencies?? new string[0])
                    {
                        if (currencyString != "")
                            currencyString += ", ";
                        currencyString += x.ToString();
                    }
                }
                return currencyString;
            }
        }

        private List<CurrencyVM> vmcurrencies = new List<CurrencyVM>();

        public List<CurrencyVM> vmCurrencies
        {
            get {
                if (vmcurrencies.Count == 0 && Currencies!=null)
                    foreach (string x in Currencies)
                        vmcurrencies.Add(new CurrencyVM { Name=x });
                return vmcurrencies; }
        }
        private List<CurrencyVM> vmgames = new List<CurrencyVM>();

        public List<CurrencyVM> vmGames
        {
            get
            {
                if (vmgames.Count == 0 && SupportedGames != null)
                    foreach (var x in SupportedGames )
                        vmgames.Add(new CurrencyVM { Name = x.ToString() });
                return vmgames;
            }
        }

        public CurrencyVM SelectedCurrency { get; set; }
        public CurrencyVM SelectedGame { get; set; }
       
    }

    public class CurrencyVM
    {
        public string Name { get; set; }
    }
}
