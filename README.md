# Dear Imgui Sample using OpenTK

## OpenTK 4

This version of the repo is targeting OpenTK 4. 
The minimum OpenGL version is 3.3.

## Your OpenGL state will be untouched!

Previous versions of this repo required users to manually reset the OpenGL state that the ImGui renderer changed. 
This is no longer needed!
The renderer resets the OpenGL state itself.



## BadImageFormatexception

If you get the following error you might have to set your platform target to x86:

```
System.BadImageFormatException: 'An attempt was made to load a program with an incorrect format. (Exception from HRESULT: 0x8007000B)'
```

To do that, right click your project (all projects referencing ImGui.NET), click "Properties", click "Build", and change the "Platform target" to "x86".

To see more info and a potential fix you can take a look at issue #2.

