This project contains the code I've written so far for the core library of what I envisioned to be DiceBot v4. 

I no longer have enough time to actively work on the project, so it's been standing still for a long time.
I'm also tired of the almost constant abuse I get for the project. I know it's a very small portion of the people that use that bot that is this way, but it's still tiring.

So, I'm uploading this project long before I intended to, to allow others to see the direction the project is going and to contribute if they want. I will review pull requests and continue to work on it when I get the time, but it will definitely not be very often.

The sites have not been fully implemented yet and some of them are outdated due to API changes since the last time I worked on this.

A summary of stuff that doormat should be capable of when finished:
-.net standard library to allow cross platform betting.
-Multi game support. The strategy and site interfaces (and programmer modes) were designed to handle multiple games, not just dice anymore.
-Multi instance in a single interface. With the new design, you can create an instance of the doormat object and it will be completely self contained, so multiple instances can be spawned in the same application, as long as the interface handles it properly.
-light distribution of updates. I had to remove some of the code handling it after moving to .net standard and I haven't fixed everything again yet, but the idea is that you can compile files that implement the site interface and the strategy interface when starting up the bot (Or while it's running), allowing one to push updates and fixes for sites and strategies without having to update the whole bot.
-Multiple programmer modes. This version already supports LUA, JS and C#. I had python support, but the ironpython libraries are not yet .net standard compatible. (They are .net core compatible though)
-More info in the programmer mode. You now have readonly access to all of the stats that DiceBot tracks internally from within the programmer mode as well as the sitestats object for your account at the site.
-Error handling within the programmer mode. For errors raised, an event will fire in the programmer mode (if your script handles it) and you can decide how to handle the error. If it is not handled, it's deferred to the settings mentioned below.
-Better error handling. You now have options for how the bot handles certain errors. You can specify the bot to continue/retry/stop on a bet error or withdrawal error etc.
-Notifications triggers. You can set up a notifications to be sent based on criteria you can specify, using any stat tracked by the bot internally. (for example if losingstreak>20 send an email and show a desktop notification. If profit is > 10% of balance, play a sound).
-Actions triggers, Like notifications, you can set up trigger actions based on any stat in the stats object and give instructions to the bot to perform an action, like stopping, withdrawing, tipping etc.
-n tier database support. When starting up the bot, you have the option to specify which database type you want to connect to and specify connection strings etc. Currently implement is sqlite, mssql, mysql, postgre. I planned to integrate mongo as well, but never got around to it. I've lightly tested sqlite, mssql and mysql and they seemed to be working fairly well. Never got around to testing postgre.
-Faster simulations. Like waaaay faster. Like 10 000 000 bet simulation in 7 minutes fast.
-Faster bets. Removed a lot of overhead of waiting for GUI updates etc, so bets might be noticibly faster for sites like yolodice, but mostly the bot will just be more resource efficient. Depends on how the UI is implemented of course.
-More user settings. Like the error handling, notifications and action triggers, give more control to the user over everything in the bot.


The api for the programmer modes have changed. Instead of having a single dobet function and a bunch of global variables, you have a reset function that is required, with a bet parameter (the name and type of bet object depends on the game you're running). The values you set for this bet parameter is your initial bet and will be the bet used when a reset is triggered in the bot. You then also have a dobet function with 3 parameters, your previous bet object, a boolean to specify whether the bet was a win or not, and a next bet object. The name of the method and types of objects for previous bet and next bet depends on the game you're playing. The values set in the nextbet obect will be used to place the next bet. You additionally have some global variables that are updated after every bet (and are read-only) that contains stats, site info and stats received from the site. (these are objects, not just variables anymore).



I've looked at UI options and haven't really decided on anything yet. There are projects like Avalonia and EtoUI, but they're still in their infancy and doesn't really have the features I require yet. They look promising but it's just not quite there yet. I also looked at usinug xamarin forms with GTKSharp, which is likely the most promising project, and it should allow me to port the program to android and ios as well. I did experience issues with depencies when using xamarin forms though, and I never really got the time to dig too deep into possible fixes. I converted the project to .netstandard for xamarin forms specifically. If another UI is used, i'd probably revert to .net core and implement the python programmer mode again.
Alternatives I've considered and are still considering is using electron connected to an mvc backend in .net core, or using edge/electron edge. Using an MVC backend with electron front end will require some significant changes in the bot to ensure all calls to and from the UI are asynchronous. Edge/electron edge also gave issues with depencies and will also probably require some rewrite to accomodate more async requests. You can see some of the preperations and testing i've done for these in the doormatcontrollers project.

There are other options that I;ve recently learned about or researched, but have not had the time to test.

On top of not finding a UI framework that I'm comfortable with and works properly, I'm really bad at UI design.





Do what you want with this project, just don't steal or scam. It's uploaded with the MIT lisence, so do whatever, but I'd like some credit for the original work. (Also, I wouldn't mind if you leave my affiliate links in the application)
