using System;
using System.ComponentModel.DataAnnotations;

namespace Gambler.Bot.Core.Sites.Classes
{
    public class Currency
    {

        public string Name { get; set; }
        [Key]
        public string Symbol { get; set; }
        public byte[] Icon { get; set; }
    }
}
