namespace Engine;

public static class Time
{
	public static float deltaTime = 0.01666666f;
	public static float editorDeltaTime = 0.01666666f;
	public static float fixedDeltaTime = 0.02f;
	public static float elapsedTime;
	public static float elapsedSeconds;
	public static ulong elapsedTicks;
	public static ulong timeScale = 0;

	public static void Update()
	{

		editorDeltaTime = (float) Window.I.UpdateTime;
		if (Global.GameRunning == false)
		{
			return;
		}

		deltaTime = (float) Window.I.UpdateTime;
		if (elapsedTicks % 5 == 0)
		{
			//Debug.Log("fps:  " + (int)(1f / deltaTime));
		}

		//if (Global.GameRunning)
		//{
		elapsedTime += deltaTime;
		elapsedSeconds = elapsedTime;
		elapsedTicks++;
		//}
	}
}