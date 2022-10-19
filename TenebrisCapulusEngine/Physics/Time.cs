namespace Tofu3D;

public static class Time
{
	public static float deltaTime = 0.01666666f;
	public static float editorDeltaTime = 0.01666666f;
	public static float fixedDeltaTime = 0.01f;
	public static float elapsedTime;
	public static float editorElapsedTime;
	public static float elapsedSeconds;
	public static ulong elapsedTicks;
	public static ulong timeScale = 0;

	public static void Update()
	{
		editorDeltaTime = (float) Window.I.UpdateTime;
		editorElapsedTime += editorDeltaTime;


		if (Global.GameRunning)
		{
			deltaTime = (float) Window.I.UpdateTime;

			elapsedTime += deltaTime;
			elapsedSeconds = elapsedTime;
			elapsedTicks++;
		}
		else
		{

			deltaTime = 0;
		}
	}
}