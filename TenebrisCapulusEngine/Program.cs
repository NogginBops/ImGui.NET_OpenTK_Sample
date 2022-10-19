using Tofu3D.Tweening;

namespace Tofu3D;

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