using Engine.Tweening;
using OpenTK.Windowing.Common;


namespace  Dear_ImGui_Sample;



public static class Program
{
	private static void Main()
	{
		_ = new Serializer();
		_ = new Scene();
		_ = new TweenManager();
		_ = new Editor();

		using (Window window = new Window())
		{

			//window.VSync = VSyncMode.Off;
			window.Run();
		}
	}
}