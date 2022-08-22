namespace Engine;

public static class Playmode
{
	public static void PlayMode_Start()
	{
		Scene.I.SaveScene();
		Global.GameRunning = true;
		Scene.I.LoadScene(Scene.I.scenePath);
	}

	public static void PlayMode_Stop()
	{
		Global.GameRunning = false;
		Scene.I.LoadScene(Scene.I.scenePath);
	}

	private static void SaveCurrentSceneBeforePlay()
	{
	}

	private static void LoadSceneSavedBeforePlay()
	{
	}
}