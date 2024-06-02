using System;
using System.ComponentModel.DataAnnotations;

namespace Gambler.Bot.Core.Sites.Classes
{
    public class SeedDetails
    {
        public string ClientSeed { get; set; }
        public string ServerSeed { get; set; }
        [Key]
        public string ServerHash { get; set; }
        public string PreviousServer { get; set; }
        public string PreviousClient { get; set; }
        public string PreviousHash { get; set; }
        public long? Nonce { get; set; }

        public SeedDetails()
        {

        }

        public SeedDetails(string Client, string Hash)
        {
            ClientSeed = Client;
            ServerHash = Hash;
        }
    }
}
