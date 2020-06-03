# Dear Imgui Sample using OpenTK

## OpenTK 4.0

This sample uses the preview packages of opentk 4.0 to show how you can use opentk 4.0 instead of 3.2 which the master branch is using.

## OpenGL 4.5

This version of the repository uses a lot of opengl 4.5 features for convenience (not having to bind buffers and stuff).

For a opengl 3.3 version of the repo using opentk 3.2 checkout the `opengl3.3` branch.

There is no version of this sample that uses opentk 4.0 and opentk 3.3.

## BadImageFormatexception

If you get the following error you might have to set your platform target to x86:

```
System.BadImageFormatException: 'An attempt was made to load a program with an incorrect format. (Exception from HRESULT: 0x8007000B)'
```

To do that, right click your project (all projects referencing ImGui.NET), click "Properties", click "Build", and change the "Platform target" to "x86".

To see more info and a potential fix you can take a look at issue #2.

## Remember to enable/disable OpenGL features after use!

Note that `ImGuiController.Render()` enables and disables some OpenGL features. The following is a list of state that `ImGuiController.Render()` changes:

- Disbles blending
- Disables scissor test
- Disables culling
- Disables depth test
- Changes blend function
- Changes scissor rectangle
- Changes the shader program in use
- Sets texture unit `0`'s `Texture2D` to `zero`
- Sets blend equation to `FuncAdd`
- Sets blend func to `SrcAlpha`, `OneMinusSrcAlpha`
- Unbinds any Vertex Array Object