using ImGuiNET;

namespace Tofu3D;

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

		ImGui.SetNextWindowSize(new Vector2(800, Window.I.ClientSize.Y - Editor.sceneViewSize.Y + 1), ImGuiCond.Always);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X - 800, Window.I.ClientSize.Y), ImGuiCond.Always, new Vector2(1, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Console", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

		if (ImGui.Button("Clear"))
		{
			Debug.ClearLogs();
		}

		int logsCount = Debug.GetLogs().Count;
		for (int i = 0; i < Mathf.Min(logsCount, Debug.LOG_LIMIT - 1); i++)
		{
			ImGui.Separator();

			string log = Debug.GetLogs()[logsCount - i - 1];
			ImGui.TextColored(new Vector4(0.74f, 0.33f, 0.16f, 1), log.Substring(0, log.IndexOf("]") + 1));
			ImGui.SameLine();

			ImGui.TextWrapped(log.Substring(log.IndexOf("]") + 1));
		}

		if (logsCount > 0)
		{
			ImGui.Separator();
		}
		//ResetID();

		ImGui.End();
	}

	public void Update()
	{
	}
}