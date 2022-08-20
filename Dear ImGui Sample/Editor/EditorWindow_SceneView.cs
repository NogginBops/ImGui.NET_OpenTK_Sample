using ImGuiNET;

namespace Engine;

public class EditorWindow_SceneView : EditorWindow
{
	public static EditorWindow_SceneView I { get; private set; }

	public override void Draw()
	{
		if (active == false)
		{
			return;
		}

		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
		if (Global.EditorAttached)
		{
			Editor.sceneViewSize = Camera.I.size + new Vector2(0, 50);

			ImGui.SetNextWindowSize(Camera.I.size + new Vector2(0, 50), ImGuiCond.Always);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
			ImGui.Begin("Scene View",
			            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

			ImGui.SetCursorPosX(Camera.I.size.X / 2 - 150);

			Vector4 activeColor = new Color(0.21f, 0.9f, 0.98f, 1f).ToVector4();
			Vector4 inactiveColor = new Color(1f, 1f, 1f, 1f).ToVector4();
			ImGui.PushStyleColor(ImGuiCol.Text, Physics.Running ? activeColor : inactiveColor);
			bool physicsButtonClicked = ImGui.Button("physics");
			if (physicsButtonClicked)
			{
				if (Physics.Running == false)
				{
					Physics.StartPhysics();
				}
				else if (Physics.Running)
				{
					Physics.StopPhysics();
				}
			}

			ImGui.PopStyleColor();

			ImGui.SameLine();

			ImGui.PushStyleColor(ImGuiCol.Text, Global.GameRunning ? activeColor : inactiveColor);

			bool playButtonClicked = ImGui.Button("play");
			if (playButtonClicked)
			{
				if (Global.GameRunning)
				{
					Playmode.PlayMode_Stop();
				}
				else
				{
					Playmode.PlayMode_Start();
				}
			}

			ImGui.PopStyleColor();

			ImGui.SameLine();
			bool resetDataButtonClicked = ImGui.Button("delete data");
			if (resetDataButtonClicked)
			{
				PersistentData.DeleteAll();
			}

			ImGui.SetCursorPosX(0);
			Editor.sceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());
			ImGui.Image((IntPtr) Window.I.sceneRenderTexture.colorAttachment, Camera.I.size,
			            new Vector2(0, 1), new Vector2(1, 0));

			ImGui.End();

			ImGui.PopStyleVar();
			ImGui.PopStyleVar();
		}
		else
		{
			ImGui.SetNextWindowSize(Camera.I.size + new Vector2(0, 50), ImGuiCond.Always);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
			ImGui.Begin("Scene View",
			            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration);

			ImGui.SetCursorPosX(0);
			Editor.sceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());
			ImGui.Image((IntPtr) Window.I.sceneRenderTexture.colorAttachment, Camera.I.size,
			            new Vector2(0, 1), new Vector2(1, 0));

			ImGui.End();
		}
	}

	public override void Update()
	{
	}

	public override void Init()
	{
		I = this;
	}
}