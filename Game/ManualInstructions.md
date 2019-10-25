### Manually

- Currently only works/tested for the Windows XNA/OpenGL version.
  - The tools themself should work on Mac/Linux
  - Celeste Studio however will not.
  
- Go to [Releases](https://github.com/ShootMe/CelesteTAS/releases)
- Download Celeste-Addons.dll (Either XNA or OpenGL, then Rename to Celeste-Addons.dll)
- You will need the modified Celeste.exe as well. I wont host it here.
  - You can modify it yourself with dnSpy or similar
  - Load Celeste.exe in dnSpy and Celeste-Addons.dll as well

![dnSpy](https://raw.githubusercontent.com/ShootMe/CelesteTAS/master/Images/dnSpy01.png)

  - Change Celeste.exe according to the document here [Modified](https://github.com/ShootMe/CelesteTAS/blob/master/Game/WhatsModified.txt)
  - First step is to modify the IL instructions of the method and add a call into TAS.Manager.UpdateInputs()
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
  - For each of the methods/fields/properties listed at the bottom of the txt file, do the following:
    - Navigate to it in dnSpy
    - Press Alt+Enter to edit it
    - Change the access type to Public.
  - Save the modified version and you should be good to go
- Place those in your Celeste game directory (usually C:\Program Files (x86)\Steam\steamapps\common\Celeste\)
- Make sure to back up the original Celeste.exe before copying. (Can rename them .bak or something)
