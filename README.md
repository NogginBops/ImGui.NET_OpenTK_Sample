# Dear Imgui Sample using OpenTK

## BadImageFormatexception

If you get the following error you might have to set your platform target to x86:

```
System.BadImageFormatException: 'An attempt was made to load a program with an incorrect format. (Exception from HRESULT: 0x8007000B)'
```

To do that, right click your project (all projects referencing ImGui.NET), click "Properties", click "Build", and change the "Platform target" to "x86".

## Remember to enable/disable OpenGL features after use!

Note that ImGuiController.Render() enables and disables some OpenGL features, such as blending, scissor testing, face culling and depth testing. It also changes the blend equation and blend function. Remember to reset these after you call ImGuiController.Render()!