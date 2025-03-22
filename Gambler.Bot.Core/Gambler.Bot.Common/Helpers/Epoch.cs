using System;
using System.Globalization;
using System.Threading;

namespace Gambler.Bot.Common.Helpers
{
    /// <summary>
    /// Used for converting from the .json data received by site
    /// Code copied from http://www.codeproject.com/Articles/272335/JSON-Serialization-and-Deserialization-in-ASP-NET
    /// </summary>
    public static class Epoch
    {
        
        public static string ToDateString(DateTime Value)
        {
            
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TimeSpan dt = Value - DateTime.Parse("1970/01/01 00:00:00", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            decimal mili = (decimal)dt.TotalMilliseconds;
            return ((long)mili).ToString();

        }

        public static DateTime ToDateTime2(string milliseconds)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            try
            {
                DateTime dotNetDate = new DateTime(1970, 1, 1);
                dotNetDate = dotNetDate.AddMilliseconds(long.Parse(milliseconds));
                if (dotNetDate.Year < 1972)
                {
                    dotNetDate = new DateTime(1970, 1, 1);
                    dotNetDate = dotNetDate.AddSeconds(long.Parse(milliseconds));
                }
                return dotNetDate;
            }
            catch
            {
                try
                {
                    string s = milliseconds.ToLower().Replace("z", " ").Replace("t", " ");
                    DateTime dotNetDate = DateTime.Parse(s, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    return dotNetDate;
                }
                catch
                {
                    return new DateTime();
                }
            }
        }
        public static string CurrentDate()
        {
            TimeSpan dt = DateTime.UtcNow - DateTime.Parse("1970/01/01 00:00:00", System.Globalization.CultureInfo.InvariantCulture);
            double mili = dt.TotalMilliseconds;
            return ((long)mili).ToString();

        }
        public static DateTime DateFromLong(long DateValue)
        {
            return new DateTime(1970, 1, 1).AddSeconds(DateValue);
        }
        public static DateTime DateFromDecimal(decimal DateValue)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds((double)(DateValue * 1000.0m));
        }
        public static long DateToLong(DateTime DateValue)
        {
            return (long)(DateValue - new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static decimal DateToDecimal(DateTime DateValue)
        {
            return (decimal)(DateValue - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000.0m;
        }
    }

}
