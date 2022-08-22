using System.Linq;
using ImGuiNET;

namespace Engine;

public class EditorWindow_Profiler : EditorWindow
{
	public static EditorWindow_Profiler I { get; private set; }

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
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X, Window.I.ClientSize.Y), ImGuiCond.Always, new Vector2(1, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Profiler", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

		ImGui.Text($"GameObjects in scene: {Scene.I.gameObjects.Count}");

		for (int i = 0; i < Debug.stats.Count; i++) ImGui.Text($"{Debug.stats.Keys.ElementAt(i)} : {Debug.stats.Values.ElementAt(i)}");

		for (int i = 0; i < Debug.timers.Count; i++)
		{
			float timerDuration = Debug.timers.Values.ElementAt(i).ElapsedMilliseconds;
			ImGui.PushStyleColor(ImGuiCol.Text, Color.Lerp(Color.White, Color.Red, Mathf.Clamp(timerDuration / 40 - 1, 0, 1)).ToVector4());
			ImGui.Text($"{Debug.timers.Keys.ElementAt(i)} : {timerDuration} ms");
			ImGui.PopStyleColor();
		}

		//ResetID();

		ImGui.End();
	}

	public override void Update()
	{
	}
}