using Engine.Tweening;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = Engine.Window;

namespace Dear_ImGui_Sample;

public static class Program
{
	private static void Main()
	{
		_ = new Serializer();
		_ = new Scene();
		_ = new TweenManager();
		_ = new SceneNavigation();
		_ = new Editor();


		using (Window window = new Window())
		{
			//window.VSync = VSyncMode.Off;
			window.Run();
		}
	}
}