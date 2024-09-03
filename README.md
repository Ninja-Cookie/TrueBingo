## TrueBingo
TrueBingo opens up the Bomb Rush Cyberfunk world, allowing you to go anywhere or do anything in any order you want from an existing file or "New Game", designed around use for Bingo gameplay.

When creating a "New Game", by default, this will place you in "Hideout" with everything avaliable to you, and all the exits accessible. From here, you can go anywhere and do anything, depending on the config you have set up.

### Keep in mind: If you load an existing file, this will make some changes on the file to accommodate for the open world nature of things, such as setting you to Chapter 6. If you don't want existing save files to have this effect applied, don't load them, and / or back them up.

Config files to edit the character, style, and outfit you start as, as well as World settings to disable or enable specific things, such as removing BMX Doors, can be found in the `BepInEx > Config > TrueBingo` folder. Editing these can be done without restarting the game, but does require some type of update to the Stage or file, such as entering a new Stage or loading / creating the file again.

## Now with BingoSync Support!
A `BingoSync.cfg` file can now be found within the `BepInEx > Config > TrueBingo` folder after launching the game.
By connecting to a room, this allows the game to automatically mark stuff on the board for you, based on the custom Json logic provided on the [GitHub](https://github.com/Ninja-Cookie/TrueBingo), with more info on how to use it.

By default, using `F1` will bring up an in-game menu to connect to a room with.

## **[Latest Version of the Json](https://pastebin.com/raw/fABW8hcK)**
To use this, when creating a room on [BingoSync](https://bingosync.com/), set the game to `Custom (Advanced)` at the bottom of the Game list, and set the Mode to `Lockout` (Which is currently the only supported mode), then set the Variant to `SRL v5` and paste the JSON from the latest version in the Board section.

From here, connect in-game using the menu (or config file with auto-connect), providing;
- The Room ID (Example Room ID: `7IkuxH2ATh6PKSZqpfE0Af` - shown on the URL after /room/),
- Room Password
- Your Player Name
- Selecting the color you will shown on the board as (which can still be changed after connecting)

For more updated info on BRC Bingo, you can join the [Discord](https://discord.gg/EWCfbJDrh4), however it is not required to use this feature.

## The full list of features include:
- Start a "New Game" and go anywhere from the start.
- Set your "New Game" starting Character, Style, and Outfit.
- Set your "New Game" Stage to spawn at, including "Random".
- Set your "New Game" Position to spawn at, including "Random".
- Toggle to Disable Story Elements.
- Toggle to Disable BMX Doors.
- Toggle to Enable Taxi Fight in Square.
- Toggle to Enable Final Boss Trigger.
- Toggle to Disable Cops.
- Toggle to Enable Teleport Robo-Posts.
- Toggle to Enable Cutscene Skip.
- Toggle to Enable Fast Cutscenes.
- Toggle to Always Show Accurate Rep.
- BingoSync Support.
