namespace Engine;

public static class KeyboardInput
{
	public static bool IsKeyDown(Keys key)
	{
		return Window.I.KeyboardState.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys) key);
	}

	public static bool IsKeyUp(Keys key)
	{
		return Window.I.KeyboardState.IsKeyReleased((OpenTK.Windowing.GraphicsLibraryFramework.Keys) key);
	}
}