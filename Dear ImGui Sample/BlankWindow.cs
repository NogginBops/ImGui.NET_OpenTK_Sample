using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Engine;

internal class BlankWindow : GameWindow
{
	public BlankWindow() : base(GameWindowSettings.Default,
	                            new NativeWindowSettings
	                            {Size = new Vector2i(1920, 1027), APIVersion = new Version(3,3), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core})
	{
	}
}