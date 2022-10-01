using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Engine;

public class EditorWindow_Profiler : EditorWindow
{
	public static EditorWindow_Profiler I { get; private set; }
	private List<float> sceneUpdateSamples = new List<float>();
	private List<float> sceneRenderSamples = new List<float>();
	private List<float> physicsThreadSamples = new List<float>();

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
			if (Debug.timers.Keys.ElementAt(i) == "Scene Update")
			{
				sceneUpdateSamples.Add(Debug.timers["Scene Update"].ElapsedMilliseconds);
				if (sceneUpdateSamples.Count > 200)
				{
					sceneUpdateSamples.RemoveAt(0);
				}
				ImGui.PlotLines("", ref sceneUpdateSamples.ToArray()[0], sceneUpdateSamples.Count, 0, $"Scene Update Time:{sceneUpdateSamples.Last()} ms            ", 0, sceneUpdateSamples.Max()+1, new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else if (Debug.timers.Keys.ElementAt(i) == "Scene Render")
			{
				sceneRenderSamples.Add(Debug.timers["Scene Render"].ElapsedMilliseconds);
				if (sceneRenderSamples.Count > 200)
				{
					sceneRenderSamples.RemoveAt(0);
				}
				ImGui.PlotLines("", ref sceneRenderSamples.ToArray()[0], sceneRenderSamples.Count, 0, $"Scene Render Time:{sceneRenderSamples.Last()} ms            ", 0, sceneRenderSamples.Max()+1, new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else if (Debug.timers.Keys.ElementAt(i) == "Physics thread")
			{
				physicsThreadSamples.Add(Debug.timers["Physics thread"].ElapsedMilliseconds);
				if (physicsThreadSamples.Count > 200)
				{
					physicsThreadSamples.RemoveAt(0);
				}
				ImGui.PlotLines("", ref physicsThreadSamples.ToArray()[0], physicsThreadSamples.Count, 0, $"Physics Update Time:{physicsThreadSamples.Last()} ms            ", 0, physicsThreadSamples.Max()+1, new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else
			{
				float timerDuration = Debug.timers.Values.ElementAt(i).ElapsedMilliseconds;
				ImGui.PushStyleColor(ImGuiCol.Text, Color.Lerp(Color.White, Color.Red, Mathf.Clamp(timerDuration / 40 - 1, 0, 1)).ToVector4());
				ImGui.Text($"{Debug.timers.Keys.ElementAt(i)} : {timerDuration} ms");
				ImGui.PopStyleColor();
			}
		}
		//ResetID();

		ImGui.End();
	}

	public override void Update()
	{
	}
}