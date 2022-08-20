using System.IO;
using ImGuiNET;

namespace Engine;

public class BrowserContextItem
{
	private Action<string> confirmAction;
	private string defaultFileName;
	private string fileExtension;
	private string itemName;
	public bool showPopup;

	public BrowserContextItem(string itemName, string defaultFileName, string fileExtension, Action<string> confirmAction)
	{
		this.itemName = itemName;
		this.defaultFileName = defaultFileName;
		this.fileExtension = fileExtension;
		this.confirmAction = confirmAction;
	}

	public void ShowContextItem()
	{
		if (ImGui.Button(itemName))
		{
			showPopup = true;
			ImGui.CloseCurrentPopup();
		}
	}

	public void ShowPopupIfOpen()
	{
		if (showPopup)
		{
			ImGui.OpenPopup(itemName);

			if (ImGui.BeginPopupContextWindow(itemName))
			{
				ImGui.InputText("", ref defaultFileName, 100);
				if (ImGui.Button("Save"))
				{
					string filePath = Path.Combine(EditorWindow_Browser.I.currentDirectory.FullName, defaultFileName + fileExtension);
					confirmAction.Invoke(filePath);

					showPopup = false;
					ImGui.CloseCurrentPopup();
				}

				ImGui.SameLine();

				if (ImGui.Button("Cancel"))
				{
					showPopup = false;
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			else
			{
				showPopup = false;
			}
		}
	}
}