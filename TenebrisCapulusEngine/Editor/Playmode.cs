﻿namespace Tofu3D;

public static class Playmode
{
	public static void PlayMode_Start()
	{
		Scene.I.SaveScene();
		Global.GameRunning = true;
		Scene.I.LoadScene(Scene.I.scenePath);

		EditorWindow_Hierarchy.I?.SelectGameObject(-1);
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