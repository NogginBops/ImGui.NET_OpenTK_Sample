using ImGuiNET;

namespace Engine;

public class EditorWindow
{
	internal bool active = true;

	private int currentID;

	internal void ResetID()
	{
		currentID = 0;
	}

	internal void PushNextID()
	{
		ImGui.PushID(currentID++);
	}

	public virtual void Init()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Draw()
	{
	}
}