using ImGuiNET;

namespace Engine;

public class EditorWindow_Console : EditorWindow
{
	public static EditorWindow_Console I { get; private set; }

	public override void Init()
	{
		I = this;
	}

	public override void Draw()
	{
		if (active == false)
		{
			return;
		}

		ImGui.SetNextWindowSize(new Vector2(Window.I.ClientSize.X / 4, Window.I.ClientSize.Y - Editor.sceneViewSize.Y + 1), ImGuiCond.Always);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X - Window.I.ClientSize.X / 4, Window.I.ClientSize.Y), ImGuiCond.Always, new Vector2(1, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Console", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

		if (ImGui.Button("Clear"))
		{
			Debug.ClearLogs();
		}

		int logsCount = Debug.GetLogs().Count;
		for (int i = 0; i < Mathf.Min(logsCount, Debug.LOG_LIMIT - 1); i++) ImGui.Text(Debug.GetLogs()[logsCount - i - 1]);
		//ResetID();

		ImGui.End();
	}

	public void Update()
	{
	}
}