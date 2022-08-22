using ImGuiNET;

namespace Engine;

public class EditorWindow_Floating : EditorWindow
{
	public static EditorWindow_Floating I { get; private set; }

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

		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Floating", ImGuiWindowFlags.NoCollapse);

		ImGui.Image((IntPtr) Window.I.sceneRenderTexture.colorAttachment, new Vector2(300, 300));

		ImGui.End();
	}

	public override void Update()
	{
	}
}