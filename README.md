# CelesteTAS
Simple TAS Tools for the game Celeste

## Installation
- Currently only works/tested for the Windows XNA/OpenGL version.
- Go to [Releases](https://github.com/ShootMe/CelesteTAS/releases)
- Download Celeste-Addons.dll (Either XNA or OpenGL, then Rename to Celeste-Addons.dll)
- You will need the modified Celeste.exe as well. I wont host it here.
  - You can modify it yourself with dnSpy or similar
  - Load Celeste.exe in dnSpy and Celeste-Addons.dll as well
  - Change Celeste.exe according to the document here [Modified](https://github.com/ShootMe/CelesteTAS/blob/master/Game/WhatsModified.txt)
  - To change it you must modify the IL instructions of the method and add a call into Celeste-Addons.dll first
  - This adds a reference to the dll and then you can just edit the method with the text in the above link.
  - You will get errors compiling, just double click the first error and delete any fields with <>
  - Save the modified version and you should be good to go
- Place those in your Celeste game directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\)
- Make sure to back up the original Celeste.exe before copying. (Can rename them .bak or something)
- For playback to be correct, make sure Jump is bound to 'A', Dash is bound to 'B', Grab is bound to 'RB', Quick Reset is bound to 'LB', and talk is bound to 'B'

## Input File
Input file is called Celeste.tas and needs to be in the main Celeste directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\Celeste.tas)

Format for the input file is (Frames),(Actions)

ie) 123,R,J (For 123 frames, hold Right and Jump)

## Actions Available
- R = Right
- L = Left
- U = Up
- D = Down
- J = Jump
- X = Dash
- G = Grab
- S = Start
- Q = Quick Reset
- F = Feather Aim
- N = Journal (Used only for Cheat Code)

## Special Input
- You can create a break point in the input file by typing *** by itself on a single line
- The program when played back from the start will try and go at 400x till it reaches that line and then go into frame stepping mode
- You can also specify the speed with ***X where X is the speedup factor. ie) ***10 will go at 10x speed

- Read,Relative File Path,Starting Line
- Will Read inputs from the specified file.
- ie) Read,1A - Forsaken City.tas,7 will read all inputs after line 7 from the '1A - Forsaken City.tas' file

## Playback / Recording of Input File
### Controller
While in game
- Playback: Right Stick
- Stop: Right Stick
- Faster Playback: Right Stick X+
- Frame Step: DPad Up
- While Frame Stepping:
  - One more frame: DPad Up
  - Continue at normal speed: DPad Down
  - Frame step continuously: Right Stick X+

### Keyboard
While in game
- Playback: Control + [
- Stop: Control + ]
- Record: Control + Backspace
- Faster Playback: Control + RightShift
- Frame Step: [
- While Frame Stepping:
  - One more frame: [
  - Continue at normal speed: ]
  - Frame step continuously: Control + RightShift
  
## Celeste Studio
Can be used instead of notepad or similar for easier editing of the TAS file. Is located in [Releases](https://github.com/ShootMe/CelesteTAS/releases) as well.

If Celeste.exe is running it will automatically open Celeste.tas if it exists. You can hit Ctrl+O to open a different file, which will automatically save it to Celeste.tas as well. Ctrl+Shift+S will open a Save As dialog as well.
