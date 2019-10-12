# CelesteTAS
Simple TAS Tools for the game Celeste

## Installation

### Everest

The easiest version to install is the [Everest version](https://github.com/EverestAPI/CelesteTAS-EverestInterop). 

- Install [Everest](https://everestapi.github.io/) if you haven't already.
- Use the 1-click installer [here](https://gamebanana.com/tools/6715)
- Enable TAS in the mod settings.

### Manually

Manual installation is only recommended if you're messing with CelesteTAS code. Instructions can be found [here.](https://github.com/ShootMe/CelesteTAS/blob/master/Game/ManualInstructions.md)

## Input File
Input file is called Celeste.tas and needs to be in the main Celeste directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\Celeste.tas)

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
- S = Start
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
- These can be rebound in TASsettings.xml, found in the main Celeste directory.
  
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

### Read
- Read,File Name,Starting Line,(Optional Ending Line)
- Will read inputs from the specified file.
- Currently requires files to be in the main Celeste directory.
- e.g. "Read,1A - Forsaken City.tas,6" will read all inputs after line 6 from the '1A - Forsaken City.tas' file
- This will also work if you shorten the file name, i.e. "Read,1A,6" will do the same 
- It's recommended to use labels instead of line numbers, so "Read,1A,lvl_1" would be the preferred format for this example.

### Labels
- Prefixing a line with # will comment out the line
- A line beginning with # can be also be used as the starting point or ending point of a Read instruction.
- You can comment highlighted text in Celeste Studio by hitting Ctrl+K
  
## Celeste Studio
Can be used instead of notepad or similar for easier editing of the TAS file. Is located in [Releases](https://github.com/ShootMe/CelesteTAS/releases) as well.

If Celeste.exe is running it will automatically open Celeste.tas if it exists. You can hit Ctrl+O to open a different file, which will automatically save it to Celeste.tas as well. Ctrl+Shift+S will open a Save As dialog as well.
