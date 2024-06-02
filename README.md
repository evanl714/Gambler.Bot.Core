Build: [![Build Status](https://eugenebotma.visualstudio.com/seuntjie900/_apis/build/status%2FSeuntjie900.Gambler.Bot.Core?branchName=master&stageName=Build)](https://eugenebotma.visualstudio.com/seuntjie900/_build/latest?definitionId=4&branchName=master)

Tests: [![Test Status](https://eugenebotma.visualstudio.com/seuntjie900/_apis/build/status%2FSeuntjie900.Gambler.Bot.Core?branchName=master&stageName=Test)](https://eugenebotma.visualstudio.com/seuntjie900/_build/latest?definitionId=4&branchName=master)

[![NuGet](https://img.shields.io/nuget/v/Gambler.Bot.Core.svg)](https://www.nuget.org/packages/Gambler.Bot.Core/)


# Gambler.Bot.Core
Gambler.Bot.Core is a class library that aims to standardize the interactions with online casinos for a variety of games, including Dice, Plinko, Roulette, Crash and more (Currently only Dice is supported). Once complete, Gambler.Bot.Core should provide an interface to sign in, get user stats, place bets, reset/setting/getting seeds, verify bets, tip/withdraw/invest/bank and potentially even chat (that's a pipe dream). For sites that has cloudflare or other DDOS protection, the library provides an event (OnBrowserBypassRequired) that must be implemented to provide cookies and a user agent that will allow connection to the site.

This project contains NO automated betting codes or strategies and aims to provide a springboard for people that want to develop their own bots by giving them access to several games at several sites in a standardized way.

At this stage, sites have only been implemented to the bare minimum (logging in, getting stats and placing Dice bets) but will be expanded upon soon after the first stable release of Gambler.Bot

# How to use

### Logging in
```
//Initialize the site you want to test
//Gambler.Bot.Core uses Microsoft.Extensions.Logging for logging. You can use any logger that implements ILogger or pass null if you do not want logging.
//Breaking change coming at some time: I will be adding open telemetry support for tracing and metrics. This change might cause changes to the constructor 
BaseSite currentSite = new Bitsler(null);//Bitsler is just an example, you can use any site from the list of supported sites


//Create a list of login parameter values
List<LoginParamValue> param = new List<LoginParamValue>();

//Iterate through the required login parameters and get the values from the user
foreach (var x in currentSite.LoginParams)
{
    Console.Write(x.Name + ": ");
    string value = Console.ReadLine();
    param.Add(new LoginParamValue
    {
        Param = x,
        Value = value
    });
}

//Log in to the site
if (await currentSite.LogIn(param.ToArray()))
{
    Console.WriteLine($"Logged in to {currentSite.SiteName}");
}
else
{
    Console.WriteLine($"Could not log in to {currentSite.SiteName}");
}
```

### Logging out
The site will have a background thread that polls for balance or stats changes occationally. To kill the thread, the user must be logged out/disconnected:
```
currentSite.Disconnect();
```

### Placing a bet
```
//Check that you are logged in and that the site supports the game you want to play:
if (currentSite.LoggedIn && currentSite is iDice diceSite) //While currentSite.SupportedGames exists, it's for display use only. The actual games are in the games interfaces
{
    DiceBet resultingBet = await diceSite.PlaceDiceBet(new PlaceDiceBet(0.00000100m, true, 49.5m));
    if (resultingBet != null)
    {
        Console.WriteLine($"Bet {resultingBet.BetID} placed. Result: {resultingBet.Profit}");
    }
    else
    {
        Console.WriteLine("Bet failed");
    }
}

//Alternatively, just use the PlaceBet Method and check for nulls or listen to the error event:
currentSite.Error += Site_Error;
Bet ResultingBet = await currentSite.PlaceBet(new PlaceDiceBet(0.00000100m, true, 49.5m));
if (resultingBet != null)
{
    Console.WriteLine($"Bet {resultingBet.BetID} placed. Result: {resultingBet.Profit}");
}
else
{
    Console.WriteLine("Bet failed, see error output for more details");
}

private void Site_Error(object sender, ErrorEventArgs e)
{
    Console.WriteLine($"Error: {e.Type.ToString()} - {e.Message}");
}
```

### The site has a list of events that can be used to facilitate asynchronous functions and provide more information about pending/failed actions:
```
currentSite.BetFinished += Site_BetFinished;
currentSite.Error += Site_Error;
currentSite.LoginFinished += Site_LoginFinished;
currentSite.Notify += Site_Notify;
currentSite.OnResetSeedFinished += Site_OnResetSeedFinished;
currentSite.OnTipFinished += Site_OnTipFinished;
currentSite.OnWithdrawalFinished += Site_OnWithdrawalFinished;
currentSite.StatsUpdated += Site_StatsUpdated;

private void Site_StatsUpdated(object sender, StatsUpdatedEventArgs e)
{
    Console.WriteLine("Stats updated: "+e.NewStats.Balance);
}

private void Site_OnWithdrawalFinished(object sender, GenericEventArgs e)
{
    Console.WriteLine("Withdrawal Finished: " + e.Success);
}

private void Site_OnTipFinished(object sender, GenericEventArgs e)
{
    Console.WriteLine("Tip Finished: " + e.Success);
}

private void Site_OnResetSeedFinished(object sender, GenericEventArgs e)
{
    Console.WriteLine("ResetSeed Finished: " + e.Success);
}

private void Site_Notify(object sender, GenericEventArgs e)
{
    Console.WriteLine("Notify Received: "+e.Message);
}

private void Site_LoginFinished(object sender, LoginFinishedEventArgs e)
{
    Console.WriteLine($"Login Finished: {e.Success}. Balance: {e.Stats?.Balance}");
}

private void Site_Error(object sender, ErrorEventArgs e)
{
    Console.WriteLine($"Error: {e.Type.ToString()} - {e.Message}");
}

private void Site_BetFinished(object sender, BetFinisedEventArgs e)
{
    Console.WriteLine("Bet Finished: " + e.NewBet.BetID);
}

```

Check out Gambler.Bot.AutoBet and Gambler.Bot for examples of how Gambler.Bot.Core has been implemented there.
