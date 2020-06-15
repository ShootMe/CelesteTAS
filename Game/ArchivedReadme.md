# CelesteTAS
Simple TAS Tools for the game Celeste

## Installation

### Everest

The easiest version to install is the [Everest version](https://github.com/EverestAPI/CelesteTAS-EverestInterop). 

- Install [Everest](https://everestapi.github.io/) if you haven't already.
- Use the 1-click installer [here.](https://gamebanana.com/tools/6715)
- Enable TAS in the mod settings.
- Enable `Unix RTC` in the mod settings and restart if on linux
- Download [Celeste Studio](https://github.com/ShootMe/CelesteTAS/releases/download/TAS/Celeste.Studio.exe), our input editor. (Note that Studio is not supported for Mac, and old versions only run on Windows)

### Manually

Manual installation is only recommended if you're messing with CelesteTAS code. Instructions can be found [here.](https://github.com/ShootMe/CelesteTAS/blob/master/Game/ManualInstructions.md)

## Input File
Input file is called Celeste.tas and needs to be in the main Celeste directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\Celeste.tas) Celeste Studio will automatically create this file for you.

Format for the input file is (Frames),(Actions)

e.g. 123,R,J (For 123 frames, hold Right and Jump)

## Actions Available
- R = Right
- L = Left
- U = Up
- D = Down
- J = Jump / Confirm
- K = Jump Bind 2
- X = Dash / Talk
- C = Dash Bind 2
- G = Grab
- S = Pause
- Q = Quick Reset
- F = Feather Aim (Format: F,angle)
- O = Confirm
- N = Journal (Used only for Cheat Code)

## Playback of Input File
### Keyboard
While in game
- Start/Stop Playback: RightControl + [
- Fast Forward / Frame Advance Continuously: RightControl + RightShift
- Pause / Frame Advance: [
- Unpause: ]
- These can be rebound in (Main Celeste Directory)\Saves\modsettings-CelesteTAS.celeste
  - Note that you may have to reload Mod Settings in Celeste for this file to appear.
  - You can also set hotkeys for modifying TAS options (e.g. showing hitboxes) in this file.
  - You can also set a default path for TAS files to be read from. (We recommend setting this to the LevelFiles folder in this repo.)
  
### Controller
While in game

- Start/Stop Playback: Right Stick
- Fast Forward / Frame Advance Continuously: Right Stick X+
- Pause / Frame Advance: DPad Up
- Unpause: DPad Down

## Special Input
### Breakpoints
- You can create a breakpoint in the input file by typing *** by itself on a single line
- The program when played back from the start will fast forward until it reaches that line and then go into frame stepping mode
- You can specify the speed with ***X, where X is the speedup factor. e.g. ***10 will go at 10x speed
- ***! will force the TAS to pause even if there are breakpoints afterward in the file

### Commands
- Various commands exist to facilitate TAS playback. Documentation can be found [here.](https://github.com/ShootMe/CelesteTAS/blob/master/Game/Commands.md)
  
## Celeste Studio
Can be used instead of notepad or similar for easier editing of the TAS file. Is located in [Releases](https://github.com/ShootMe/CelesteTAS/releases) as well.

If Celeste.exe is running it will automatically open Celeste.tas if it exists. You can hit Ctrl+O to open a different file, which will automatically save it to Celeste.tas as well. Ctrl+Shift+S will open a Save As dialog as well.
