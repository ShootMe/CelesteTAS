# CelesteTAS
Simple TAS Tools for the game Celeste

## Installation

### Everest

The easiest way to install is through the [Everest interop mod](https://github.com/EverestAPI/CelesteTAS-EverestInterop). 

- [Download the zip from here](https://github.com/EverestAPI/CelesteTAS-EverestInterop/releases)
- Place it in your `Mods` directory
- [Download the TAS addon here](https://github.com/ShootMe/CelesteTAS/releases), either Celeste-Addons-OpenGL.dll or Celeste-Addons-XNA.dll, whichever corresponds with your version of Celeste
- Place it in the same directory as `Celeste.exe`, and rename it to `Celeste-Addons.dll`
- Enable TAS in the mod settings.

For playback to be correct, make sure Jump is bound to 'A' and 'Y', Dash is bound to 'B' and 'X', Grab is bound to 'RB', Quick Reset is bound to 'LB', and talk is bound to 'B'. If you don't have a controller, you have to edit the settings file manually. Here's the relevant section:

```
  <BtnGrab>
    <Buttons>RightTrigger</Buttons>
    <Buttons>RightShoulder</Buttons>
  </BtnGrab>
  <BtnJump>
    <Buttons>A</Buttons>
    <Buttons>Y</Buttons>
  </BtnJump>
  <BtnDash>
    <Buttons>X</Buttons>
    <Buttons>B</Buttons>
  </BtnDash>
  <BtnTalk>
    <Buttons>B</Buttons>
  </BtnTalk>
  <BtnAltQuickRestart>
    <Buttons>LeftTrigger</Buttons>
    <Buttons>LeftShoulder</Buttons>
  </BtnAltQuickRestart>
```

### Manually

- Currently only works/tested for the Windows XNA/OpenGL version.
- Go to [Releases](https://github.com/ShootMe/CelesteTAS/releases)
- Download Celeste-Addons.dll (Either XNA or OpenGL, then Rename to Celeste-Addons.dll)
- You will need the modified Celeste.exe as well. I wont host it here.
  - You can modify it yourself with dnSpy or similar
  - Load Celeste.exe in dnSpy and Celeste-Addons.dll as well

![dnSpy](https://raw.githubusercontent.com/ShootMe/CelesteTAS/master/Images/dnSpy01.png)

  - Change Celeste.exe according to the document here [Modified](https://github.com/ShootMe/CelesteTAS/blob/master/Game/WhatsModified.txt)
  - First step is to modfy the IL instructions of the method and add a call into TAS.Manager.UpdateInputs()
  - Find the Monocole.Engine.Update(GameTime) method in dnSpy

![dnSpy](https://raw.githubusercontent.com/ShootMe/CelesteTAS/master/Images/dnSpy02.png)

  - Right click in the right hand window and select Edit IL Instructions...
  - In this window right click and select Add New Instruction...

![dnSpy](https://raw.githubusercontent.com/ShootMe/CelesteTAS/master/Images/dnSpy03.png)

  - Change the OpCode to be 'call' and then click the 'null' in the operand and select 'Method' then browse to Celeste-Addons.TAS.Manager.UpdateInputs
  - This adds a reference to Celeste-Addons.dll
  - Click OK in the Edit Method Body window
  - Then go to File -> Save Module and save this modified exe
  - Then go to File -> Reload All Assemblies to load the modified exe
  - Go back to the Monocle.Engine.Update method
  - This time right click in the right window and select 'Edit Method (C#)'
  - Replace the body of the method with the body in the txt file linked above
  - Go to the Celeste.RunThread.Start method
  - Right click in the right window and select 'Edit Method (C#)'
  - Replace the body of the method with the body in the txt file linked above
  - Save the modified version and you should be good to go
- Place those in your Celeste game directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\)
- Make sure to back up the original Celeste.exe before copying. (Can rename them .bak or something)

## Input File
Input file is called Celeste.tas and needs to be in the main Celeste directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\Celeste.tas)

Format for the input file is (Frames),(Actions)

ie) 123,R,J (For 123 frames, hold Right and Jump)

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
- F = Feather Aim
- O = Confirm
- N = Journal (Used only for Cheat Code)

## Special Input
- You can create a break point in the input file by typing *** by itself on a single line
- The program when played back from the start will try and go at 400x till it reaches that line and then go into frame stepping mode
- You can also specify the speed with ***X where X is the speedup factor. ie) ***10 will go at 10x speed

- Read,Relative File Path,Starting Line
- Will Read inputs from the specified file.
- ie) Read,1A - Forsaken City.tas,7 will read all inputs after line 7 from the '1A - Forsaken City.tas' file

## Playback of Input File
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
- Playback: RightControl + [
- Stop: RightControl + [
- Faster Playback: RightControl + RightShift
- Frame Step: [
- While Frame Stepping:
  - One more frame: [
  - Continue at normal speed: ]
  - Frame step continuously: RightControl + RightShift
  
## Celeste Studio
Can be used instead of notepad or similar for easier editing of the TAS file. Is located in [Releases](https://github.com/ShootMe/CelesteTAS/releases) as well.

If Celeste.exe is running it will automatically open Celeste.tas if it exists. You can hit Ctrl+O to open a different file, which will automatically save it to Celeste.tas as well. Ctrl+Shift+S will open a Save As dialog as well.
