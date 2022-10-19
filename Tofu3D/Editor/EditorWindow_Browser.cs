using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorWindow_Browser : EditorWindow
{
	private string[] assets = new string[0];

	private List<BrowserContextItem> contextItems;

	public DirectoryInfo currentDirectory;
	private Texture directoryIcon;

	private Texture fileIcon;

	private Texture[] textures = new Texture[0];
	public static EditorWindow_Browser I { get; private set; }

	private Vector2 iconSize = new Vector2(98, 90) * Global.EditorScale;

	public override void Init()
	{
		I = this;

		CreateContextItems();

		fileIcon = new Texture();
		fileIcon.Load("Resources/FileIcon.png", false);

		directoryIcon = new Texture();
		directoryIcon.Load("Resources/DirectoryIcon.png", false);

		currentDirectory = new DirectoryInfo("Assets");

		RefreshAssets();
	}

	private void CreateContextItems()
	{
		BrowserContextItem createSceneContextItem = new("Create Scene", "scene", ".scene", filePath =>
		{
			Scene.I.CreateEmptySceneAndOpenIt(filePath);
			RefreshAssets();
		});
		BrowserContextItem createMaterialContextItem = new("Create Material", "mat", ".mat", filePath =>
		{
			Material createdMaterial = new();
			createdMaterial.path = filePath;
			MaterialAssetManager.SaveMaterial(createdMaterial);
			RefreshAssets();
		});
		contextItems = new List<BrowserContextItem>
		               {createSceneContextItem, createMaterialContextItem};
	}

	public override void Update()
	{
	}

	private void RefreshAssets()
	{
		if (Directory.Exists(currentDirectory.FullName) == false)
		{
			return;
		}

		string[] tmp_assets = Directory.GetDirectories(currentDirectory.FullName);
		List<string> allAssets = tmp_assets.Concat(Directory.GetFiles(currentDirectory.FullName, "", SearchOption.TopDirectoryOnly)).ToList();

		for (int i = 0; i < allAssets.Count; i++)
		{
			if (Path.GetFileName(allAssets[i]).StartsWith('.'))
			{
				allAssets.RemoveAt(i);
				i--;
				continue;
			}
		}

		assets = allAssets.ToArray();

		for (int i = 0; i < textures.Length; i++)
			if (textures[i] != null && textures[i].loaded)
			{
				textures[i].Delete();
			}

		textures = new Texture[assets.Length];
		for (int i = 0; i < assets.Length; i++)
		{
			string assetExtension = Path.GetExtension(assets[i]).ToLower();

			if (assetExtension.ToLower().Contains(".jpg") || assetExtension.ToLower().Contains(".png") || assetExtension.ToLower().Contains(".jpeg"))
			{
				textures[i] = new Texture();
				textures[i].Load(assets[i], false);
			}
		}
	}

	public override void Draw()
	{
		if (active == false)
		{
			return;
		}

		ImGui.SetNextWindowSize(new Vector2(Window.I.ClientSize.X - 1600, Window.I.ClientSize.Y - Editor.sceneViewSize.Y + 1), ImGuiCond.Always);
		ImGui.SetNextWindowPos(new Vector2(0, Window.I.ClientSize.Y), ImGuiCond.Always, new Vector2(0, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Browser", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
		ResetID();

		if (ImGui.Button("<"))
		{
			currentDirectory = currentDirectory.Parent;
			RefreshAssets();
		}

		ResetID();
		if (Editor.I.GetSelectedGameObject() != null)
		{
			PushNextID();
			bool saveBtnPressed = ImGui.Button("Save Prefab");
			if (saveBtnPressed)
			{
				if (Directory.Exists("Assets/Prefabs") == false)
				{
					Directory.CreateDirectory("Assets/Prefabs");
				}

				Serializer.I.SaveGameObject(Editor.I.GetSelectedGameObject(), "Assets/Prefabs/" + Editor.I.GetSelectedGameObject().name + ".prefab");
			}
		}

		//for (int i = 0; i < assets.Length; i++)
		//{
		//	if (i > 0)
		//	{
		//		ImGui.SameLine();
		//	}
		//	ImGui.BeginGroup();
		//	string directoryName = new DirectoryInfo(directories[i]).Name;
		//	PushNextID();
		//
		//
		//	ImGui.PushStyleColor(ImGuiCol.Button, new Color(13, 27, 30).ToVector4());
		//	bool directoryClicked = ImGui.Button("FOLDER", new Vector2(100, 100));
		//	ImGui.PopStyleColor();
		//	if (directoryClicked)
		//	{
		//		currentDirectory = new DirectoryInfo(directories[i]);
		//		RefreshAssets();
		//		return;
		//	}
		//
		//	ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
		//	ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);
		//
		//	string a = directoryName.Substring(0, Math.Clamp(directoryName.Length, 1, 12));
		//	ImGui.Text(a);
		//
		//
		//	ImGui.EndGroup();
		//}
		for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
		{
			if (assetIndex != 0 && assetIndex % 6 != 0)
			{
				ImGui.SameLine();
			}

			DirectoryInfo directoryInfo = new(assets[assetIndex]);
			bool isDirectory = directoryInfo.Exists;

			ImGui.BeginGroup();
			string assetName = Path.GetFileNameWithoutExtension(assets[assetIndex]);
			string assetExtension = Path.GetExtension(assets[assetIndex]).ToLower();
			PushNextID();

			//ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0,0,0,0));

			if (isDirectory)
			{
				ImGui.ImageButton((IntPtr) directoryIcon.id, iconSize);
				//ImGui.ImageButton((IntPtr) 0, new Vector2(100, 90));
			}
			else
			{
				if (textures[assetIndex] != null && textures[assetIndex].loaded)
				{
					ImGui.ImageButton((IntPtr) textures[assetIndex].id, iconSize);
				}
				else
				{
					//ImGui.ImageButton((IntPtr) fileIcon.id, new Vector2(100, 90));
					ImGui.ImageButton((IntPtr) fileIcon.id, iconSize);
				}
			}
			//ImGui.PopStyleColor();


			if (assetExtension.ToLower().Contains(".jpg") || assetExtension.ToLower().Contains(".png") || assetExtension.ToLower().Contains(".jpeg"))
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("CONTENT_BROWSER_TEXTURE", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) textures[assetIndex].id, iconSize);

					//ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			if (assetExtension.ToLower().Contains(".mp3") || assetExtension.ToLower().Contains(".wav"))
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) fileIcon.id, iconSize);


					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}


			bool isMaterial = assetExtension.ToLower().Contains(".mat");
			bool isShader = assetExtension.ToLower().Contains(".glsl");
			bool isPrefab = assetExtension.ToLower().Contains(".prefab");
			if (isShader || isMaterial)
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					if (isMaterial)
					{
						ImGui.SetDragDropPayload("CONTENT_BROWSER_MATERIAL", stringPointer, (uint) (sizeof(char) * itemPath.Length));
					}

					if (isShader)
					{
						ImGui.SetDragDropPayload("CONTENT_BROWSER_SHADER", stringPointer, (uint) (sizeof(char) * itemPath.Length));
					}

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) fileIcon.id, new Vector2(100, 90));

					//ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			if (isPrefab)
			{
				if (ImGui.BeginDragDropSource())
				{
					string itemPath = assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("PREFAB_PATH", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					//string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) fileIcon.id, iconSize);

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
			{
				if (assetExtension.ToLower().Contains(".mat"))
				{
					EditorWindow_Inspector.I.OnMaterialSelected(assets[assetIndex]);
				}
			}

			if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
			{
				if (isDirectory)
				{
					currentDirectory = directoryInfo;
					RefreshAssets();
					return;
				}

				if (assetExtension == ".prefab")
				{
					GameObject go = Serializer.I.LoadPrefab(assets[assetIndex]);
					EditorWindow_Hierarchy.I.SelectGameObject(go.id);
				}

				if (assetExtension == ".scene")
				{
					Scene.I.LoadScene(assets[assetIndex]);
				}
			}

			//ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
			//ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

			string a = assetName.Substring(0, Math.Clamp(assetName.Length, 0, 12));
			ImGui.Text(a);
			ImGui.EndGroup();
		}


		// show prefabs as btns from array that updates in Update()
		if (ImGui.BeginPopupContextWindow("BrowserPopup"))
		{
			for (int i = 0; i < contextItems.Count; i++)
			{
				contextItems[i].ShowContextItem();
			}
			/*if (ImGui.Button("New Scene"))
			{
				showCreateScenePopup = true;
				ImGui.CloseCurrentPopup();
			}

			if (ImGui.Button("New Material"))
			{
				showCreateMaterialPopup = true;
				ImGui.CloseCurrentPopup();
			}*/

			ImGui.EndPopup();
		}

		for (int i = 0; i < contextItems.Count; i++)
		{
			contextItems[i].ShowPopupIfOpen();
		}


		ImGui.End();
	}

	public void GoToFile(string directory)
	{
		if (File.Exists(directory) == false)
		{
			return;
		}

		currentDirectory = Directory.GetParent(directory);
	}
}