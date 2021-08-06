using DoormatCore.Games;
using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore
{
    public static class testSample
    {
        static BaseSite Site;
        public static void Main(string[] args)
        {
            //Initialize the site you want to test
             Site = new Bitsler();
            //Create a list of param values
            List<BaseSite.LoginParamValue> param = new List<BaseSite.LoginParamValue>();
            
            foreach (var x in Site.LoginParams)
            {
                Console.Write(x.Name + ": ");
                string value = Console.ReadLine();
                param.Add(new BaseSite.LoginParamValue
                {
                    Param = x,
                    Value = value
                });
            }

            //subscribe to site events
            Site.BetFinished += Site_BetFinished;
            Site.Error += Site_Error;
            Site.LoginFinished += Site_LoginFinished;
            Site.Notify += Site_Notify;
            Site.OnResetSeedFinished += Site_OnResetSeedFinished;
            Site.OnTipFinished += Site_OnTipFinished;
            Site.OnWithdrawalFinished += Site_OnWithdrawalFinished;
            Site.StatsUpdated += Site_StatsUpdated;
            
            //Log in using the params
            Site.LogIn(param.ToArray());
            Site.Currency = 1;
            Console.WriteLine("Wait for site to finish logging in then press enter");
            Console.ReadLine();
            Console.WriteLine("Placing test bet");
            //Site.PlaceBet(new PlaceDiceBet(100000001, false,75 ));
            NextTest();
            Console.WriteLine("Press enter to reset seed");
            Console.ReadLine();            
            Site.ResetSeed();
        }
        /*
         * Test Definitions:
         * 0: Balance too low
         * 1: 0 Bet
         * 2: Chance too high
         * 3: Chance too low
         * 4: ResetSeed
         * 5: Withdraw too much         
         * 6: Tip
         * */
        static int TestCount = 0;
        static void NextTest()
        {
            Console.WriteLine("Test "+TestCount+":");
            switch (TestCount++)
            {
                case 0: Site.PlaceBet(new PlaceDiceBet(100000001, false, 75)); break;
                case 1: Site.PlaceBet(new PlaceDiceBet(0, false, 75)); break;
                case 2: Site.PlaceBet(new PlaceDiceBet(0.0000010m, false, 99.9999m)); break;
                case 3: Site.PlaceBet(new PlaceDiceBet(0.0000010m, false, 0)); break;
                case 4: Site.ResetSeed();break;
                case 5: Console.Write("Withdrawal Address: ");
                    string address = "DGcBmED7Dyes4R7c1sP7ETc97VpRWuSh4r";// Console.ReadLine(); 
                    Site.Withdraw(address,1000000); break;
               
                case 6:
                    Console.Write("Tip user: ");
                    string user = "asfdkljhasd"; //Console.ReadLine(); 
                    Site.SendTip(user, 1000000); break;
            }
        }

        private static void Site_StatsUpdated(object sender, StatsUpdatedEventArgs e)
        {
            Console.WriteLine("Stats updated");
            //throw new NotImplementedException();
        }

        private static void Site_OnWithdrawalFinished(object sender, GenericEventArgs e)
        {
            Console.WriteLine("Withdrawal Finished: " + e.Success);
            NextTest();
        }

        private static void Site_OnTipFinished(object sender, GenericEventArgs e)
        {
            Console.WriteLine("Tip Finished: " + e.Success);
            NextTest();
        }

        private static void Site_OnResetSeedFinished(object sender, GenericEventArgs e)
        {
            Console.WriteLine("ResetSeed Finished: " + e.Success);
            NextTest();
        }

        private static void Site_Notify(object sender, GenericEventArgs e)
        {
            //Console.WriteLine("Notify Received: "+e.Message);
            //throw new NotImplementedException();
        }

        private static void Site_LoginFinished(object sender, LoginFinishedEventArgs e)
        {
            Console.WriteLine("Login Finished. Press enter to place a bet");
        }

        private static void Site_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"Error: {e.Type.ToString()} - {e.Message}");
            NextTest();
        }

        private static void Site_BetFinished(object sender, BetFinisedEventArgs e)
        {
            Console.WriteLine("Bet Finished: " + e.NewBet.BetID);
            NextTest();
        }
    }
}
