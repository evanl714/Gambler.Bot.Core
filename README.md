Build: [![Build Status](https://eugenebotma.visualstudio.com/seuntjie900/_apis/build/status%2FDoormat?branchName=master&stageName=Build)](https://eugenebotma.visualstudio.com/seuntjie900/_build/latest?definitionId=2&branchName=master)

Tests: [![Build Status](https://eugenebotma.visualstudio.com/seuntjie900/_apis/build/status%2FDoormat?branchName=master&stageName=Test)](https://eugenebotma.visualstudio.com/seuntjie900/_build/latest?definitionId=2&branchName=master)

[![NuGet](https://img.shields.io/nuget/v/DoormatCore.svg)](https://www.nuget.org/packages/DoormatCore/)

This project contains the code I've written so far for the core library of what I envisioned to be DiceBot v4. 

This project will contain only code relating to site APIs and attempt to provide a standardized API for logging in and placing bets at different crypto casinos. It will contain NO automated betting codes. The aim is to provide a springboard for people that want to develop their own bots by giving them access to several games at several sites in a standardized way.

The sites have not been fully implemented yet and some of them are outdated due to API changes since the last time I worked on this.

# How to use

### Logging in
```
//Initialize the site you want to test
BaseSite currentSite = new Bitsler();

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





A summary of stuff that doormat and krygames bot should be capable of when finished (most of the below is not relevant to the doormat repository):

- .net standard library to allow cross platform betting.
- Multi game support. The strategy and site interfaces (and programmer modes) were designed to handle multiple games, not just dice anymore.
- Multi instance in a single interface. With the new design, you can create an instance of the doormat object and it will be completely self contained, so multiple instances can be spawned in the same application, as long as the interface handles it properly.
- light distribution of updates. I had to remove some of the code handling it after moving to .net standard and I haven't fixed everything again yet, but the idea is that you can compile files that implement the site interface and the strategy interface when starting up the bot (Or while it's running), allowing one to push updates and fixes for sites and strategies without having to update the whole bot.
- Multiple programmer modes. This version already supports LUA, JS and C#. I had python support, but the ironpython libraries are not yet .net standard compatible. (They are .net core compatible though)
- More info in the programmer mode. You now have readonly access to all of the stats that DiceBot tracks internally from within the programmer mode as well as the sitestats object for your account at the site.
- Error handling within the programmer mode. For errors raised, an event will fire in the programmer mode (if your script handles it) and you can decide how to handle the error. If it is not handled, it's deferred to the settings mentioned below.
- Better error handling. You now have options for how the bot handles certain errors. You can specify the bot to continue/retry/stop on a bet error or withdrawal error etc.
- Notifications triggers. You can set up a notifications to be sent based on criteria you can specify, using any stat tracked by the bot internally. (for example if losingstreak>20 send an email and show a desktop notification. If profit is > 10% of balance, play a sound).
- Actions triggers, Like notifications, you can set up trigger actions based on any stat in the stats object and give instructions to the bot to perform an action, like stopping, withdrawing, tipping etc.
- n tier database support. When starting up the bot, you have the option to specify which database type you want to connect to and specify connection strings etc. Currently implement is sqlite, mssql, mysql, postgre. I planned to integrate mongo as well, but never got around to it. I've lightly tested sqlite, mssql and mysql and they seemed to be working fairly well. Never got around to testing postgre.
- Faster simulations. Like waaaay faster. Like 10 000 000 bet simulation in 7 minutes fast.
- Faster bets. Removed a lot of overhead of waiting for GUI updates etc, so bets might be noticibly faster for sites like yolodice, but mostly the bot will just be more resource efficient. Depends on how the UI is implemented of course.
- More user settings. Like the error handling, notifications and action triggers, give more control to the user over everything in the bot.
- KeePass 2 integration. The keepass project is pulled from https://github.com/Strangelovian/KeePass2Core. I couldn't get it to work properly just referencing the dll, so I just pulled in the project. (I spent a whole 5 minutes of trying though. I'm sure someone can do better)

The api for the programmer modes have changed. Instead of having a single dobet function and a bunch of global variables, you have a reset function that is required, with a bet parameter (the name and type of bet object depends on the game you're running). The values you set for this bet parameter is your initial bet and will be the bet used when a reset is triggered in the bot. You then also have a dobet function with 3 parameters, your previous bet object, a boolean to specify whether the bet was a win or not, and a next bet object. The name of the method and types of objects for previous bet and next bet depends on the game you're playing. The values set in the nextbet obect will be used to place the next bet. You additionally have some global variables that are updated after every bet (and are read-only) that contains stats, site info and stats received from the site. (these are objects, not just variables anymore).

There's a programmer mode doc and a readme file in the doormat project that provides more information on how the triggers and settings work, and the new programmer mode API. There's also some sample scripts for each of the programmer mod that might or might not work.

UI is being done in avalonia at this stage.

There are other options that I;ve recently learned about or researched, but have not had the time to test.

On top of not finding a UI framework that I'm comfortable with and works properly, I'm really bad at UI design.
