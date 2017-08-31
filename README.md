# Age of Empires 2 Lobby Companion

Lobby Companion is a **desktop app that works with Age of Empires 2 HD** and provides additional info for players who join your lobby. It works for both lobby hosting and lobby joining scenarios.
Additional info that is being displayed is **Location, Total Games, Win Ratio and Drop Ratio** so you can make a more educated guess with whom you would like to play with.
To keep the track of players who you don't want to play with in the future or who you would like to **you can use a reputation system**.
Reputation system **allows to assign negative or positive reputation** for players who you played the game with so the next time the same player joins your lobby you can **check the reputation history** of that player and decide whether to let that person stay or not.
**Players are tracked by SteamID**, so evasion is not possible by changing a name. To make the experience more nicer Lobby Companion is capable of **projecting in-game overlay** with all the information you need to make a decision about the player without having to alt-tab into the actual Lobby Companion app.
Optionally (which is enabled by default) overlay is also shown when the game is not active, so you can do other stuff while waiting for your lobby to get full and still keep an eye on the lobby progress without constantly alt-tabbing to check if the lobby is full. Additional features include **balanced team calculation** either based on Rank (ELO), Total Games or Win Ratio.
**This tool does not require to log in with your Steam account nor require to provide any credentials**, it works side by side with your running Steam instance.

Short link for sharing: [aoe2lc.net](http://aoe2lc.net)

## [Reddit](https://www.reddit.com/r/aoe2lc)

## Screenshots
* [[UI] Running lobby session (v1.0.0)](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/LobbySessionRunning.png)
* [[Overlay] In-game](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/InGameOverlay.png)
* [[Overlay] Always on, over desktop](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/PermanentOverlay.png)
* [[UI] Assign reputation (v1.0.0)](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/AssignReputation.png)
* [[UI] Player profile (v1.0.0)](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/PlayerProfile.png)
* [[UI] Calculate balanced teams (v1.1.0)](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/CalculateBalancedTeams.png)
* [[UI] Reputations page (v1.2.0)](https://raw.githubusercontent.com/ThorConzales/AoE2HDLobbyCompanion/master/Screenshots/ReputationsPage.png)

## Download
[Download v1.1.0](https://github.com/ThorConzales/AoE2HDLobbyCompanion/releases/download/v1.1.0/AoE2HDLobbyCompanion-V1.1.0.zip)

**Minimum Requirements**: Windows 7 SP1 or newer

**Prerequisites**
* [.NET Framework 4.6.1](https://www.microsoft.com/net/download/framework) (Updated Windows 10 users already have it, if you don't know if you have it try starting up /Backend/Backend.exe and see if it complains about missing .NET Framework)
* [Redistributable C++ Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53587)

## How to use it
Just download the ZIP, extract it and launch AoE2HDLobbyCompanion.exe while your Steam Client is also running. When you are planning to play click on "Start Session". When you join a lobby or host a lobby you should see a lobby state being reflected in Lobby Companion app. 
When the game starts Lobby Companion should automatically mark the lobby as started and close the overlay in 1 minute into the game. To do it sooner you can click on "Start the lobby manually". When the game has finished you must seal the lobby before starting a new session.
Befor sealing the lobby make sure you have assigned reputations. You can start assigning reputations even before the lobby has started. 
Lobby Companion tries to be as flexible as possible in terms of workflow. It's main purpose is to be your personal notepad when it comes to reputations and it makes no attempt to restrict you, therefor you can perform invalid actions without Lobby Companion stopping you, for example starting the lobby manually without the actual game being started.
First time users have to go through a setup process where among other things most common problems are being explained and how to solve them. **It is strongly recommended to go through the setup process with attention and not to skip anything.**

## How it works
Lobby Companion injects a [NetHook2](https://github.com/SteamRE/SteamKit/tree/master/Resources/NetHook2) DLL into Steam process to tap into Steam Client's network stream. That's how it is capable of knowing who the players are. 
There are 3 distinct components that work together: AoE2HDLobbyCompanion.exe, Backend.exe and NetHook2.dll:
* AoE2HDLobbyCompanion.exe is a main process and a UI layer that is built with [Electron](https://electron.atom.io/). It also spawns Backend.exe and NetHook2.dll processes, because it is not powerful enough to peform all the tasks on its own. Because it is spawning additional processes some Anti-Viruses may flag those processes as malicious activities and therefor block them. To avoid Anti-Virus interruptions you have to add exclusions for those files. If you scan those files separately you should see that there is nothing malicious about them.
* Backend.exe is a [Kestrel web server](https://github.com/aspnet/KestrelHttpServer) built with .NET Core running on port 5000. It is a web server so the AoE2HDLobbyCompanion.exe could communicate with it. It's main purpose is to read NetHook2's network dump, deal with all the database operations and project an overlay.
* NetHook2.dll purpose is to be injected into Steam Client's address space so the network traffic could be dumped for further inspection.

Database is stored separately from the installation folder to make the sure the database is not lost while upgrading. To make a backup of your database make a copy of C:\Users\\[user]\AppData\Roaming\AoE2HDLobbyCompanion (AppData is hidden folder).

## Known Issues
* Some Anti-Viruses might block NetHook2 DLL when a session is started. If it happens your Steam client along with the game will crash. To prevent it happening again you have to add an exclusion to your Anti-Virtus to prevent it from scanning NetHook2 process in the future.
* Sometimes it can happen that it won't detect that you are in the lobby. If it happens then for some reason NetHook2 injection is not working properly. To fix it stop the lobby session, restart Steam client and start the session again, hopefully this time it works. It can also happen it won't work multiple times in a row. To tell if it is working without having to start up the game, start the session, click around in Steam client and if it works properly then to the console that is opened when you started the session should start printing out some logs when you click around.


## Wiki
[Building and Development](https://github.com/ThorConzales/AoE2HDLobbyCompanion/wiki/Building-and-Development)
