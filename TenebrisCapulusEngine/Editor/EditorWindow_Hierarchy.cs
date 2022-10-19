using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorWindow_Hierarchy : EditorWindow
{
	private bool canDelete = true;

	private GameObject clipboardGameObject;
	private int gameObjectIndexSelectedBefore;
	public Action<int> GameObjectSelected;
	private List<GameObject> gameObjectsChildrened = new();

	private int selectedGameObjectIndex;
	private bool showUpdatePrefabPopup;
	public static EditorWindow_Hierarchy I { get; private set; }

	public override void Init()
	{
		I = this;
	}

	public override void Update()
	{
		if (KeyboardInput.IsKeyDown(Keys.Delete) && canDelete)
		{
			canDelete = false;
			DestroySelectedGameObjects();
		}

		if (KeyboardInput.IsKeyUp(Keys.Delete))
		{
			canDelete = true;
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.IsKeyUp(Keys.C))
		{
			if (Editor.I.GetSelectedGameObject() != null)
			{
				clipboardGameObject = Editor.I.GetSelectedGameObject();
				Serializer.I.SaveClipboardGameObject(clipboardGameObject);
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.IsKeyUp(Keys.V))
		{
			if (clipboardGameObject != null)
			{
				GameObject loadedGO = Serializer.I.LoadClipboardGameObject();
				SelectGameObject(loadedGO.id);
			}
		}
	}

	private void DestroySelectedGameObjects()
	{
		foreach (GameObject selectedGameObject in Editor.I.GetSelectedGameObjects())
		{
			selectedGameObject.Destroy();
			selectedGameObjectIndex--;
			if (selectedGameObjectIndex < 0)
			{
				return;
			}

			GameObjectSelected.Invoke(Scene.I.gameObjects[selectedGameObjectIndex].id);
		}
	}

	private void MoveSelectedGameObject(int addToIndex = 1)
	{
		int direction = addToIndex;
		if (Editor.I.GetSelectedGameObjects().Count == 0)
		{
			return;
		}

		GameObject go = Editor.I.GetSelectedGameObjects()[0];
		int oldIndex = go.indexInHierarchy;

		if (oldIndex + direction >= Scene.I.gameObjects.Count || oldIndex + direction < 0)
		{
			return;
		}

		while (Scene.I.gameObjects[oldIndex + direction].transform.parent != null) direction += addToIndex;

		Scene.I.gameObjects.RemoveAt(oldIndex);
		Scene.I.gameObjects.Insert(oldIndex + direction, go);

		selectedGameObjectIndex = oldIndex + direction;
		GameObjectSelected.Invoke(Scene.I.gameObjects[oldIndex + direction].id);
	}

	public void SelectGameObject(int id)
	{
		selectedGameObjectIndex = Editor.I.GetGameObjectIndexInHierarchy(id);
		GameObjectSelected.Invoke(id);
		//Debug.Log("Selected go: " + id);
	}

	public override void Draw()
	{
		if (active == false)
		{
			return;
		}

		ResetID();
		windowWidth = 700;
		ImGui.SetNextWindowSize(new Vector2(windowWidth, Editor.sceneViewSize.Y), ImGuiCond.Always);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X - EditorWindow_Inspector.I.windowWidth, 0), ImGuiCond.Always, new Vector2(1, 0)); // +1 for double border uglyness
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
		if (ImGui.Button("+"))
		{
			GameObject go = GameObject.Create(name: "GameObject");
			go.Awake();
			go.transform.position = Camera.I.CenterOfScreenToWorld();
		}

		ImGui.SameLine();

		if (ImGui.Button("-"))
		{
			DestroySelectedGameObjects();
		}

		ImGui.SameLine();
		ImGui.Dummy(new Vector2(15, 0));
		ImGui.SameLine();
		if (ImGui.Button("^"))
		{
			MoveSelectedGameObject(-1);
		}

		ImGui.SameLine();
		if (ImGui.Button("V"))
		{
			MoveSelectedGameObject();
		}

		ImGui.SameLine();
		if (ImGui.Button("Add children"))
		{
			GameObject go = GameObject.Create(name: "Children");
			go.Awake();
			go.transform.SetParent(Scene.I.gameObjects[selectedGameObjectIndex].transform);
		}

		for (int goIndex = 0; goIndex < Scene.I.gameObjects.Count; goIndex++)
		{
			DrawGameObjectRow(goIndex);
		}

		ImGui.End();
	}

	private void DrawGameObjectRow(int goIndex, bool isChild = false)
	{
		GameObject currentGameObject = Scene.I.gameObjects[goIndex];
		if (currentGameObject.transform.parent != null && isChild == false) // only draw children from recursive DrawGameObjectRow calls
		{
			return;
		}

		if (currentGameObject.silent)
		{
			return;
		}

		//bool hasAnyChildren = false;
		bool hasAnyChildren = currentGameObject.transform.children?.Count > 0;
		ImGuiTreeNodeFlags flags = (selectedGameObjectIndex == goIndex ? ImGuiTreeNodeFlags.Selected : 0) | ImGuiTreeNodeFlags.OpenOnArrow;
		if (hasAnyChildren == false)
		{
			flags = (selectedGameObjectIndex == goIndex ? ImGuiTreeNodeFlags.Selected : 0) | ImGuiTreeNodeFlags.Leaf;
		}


		Vector4 nameColor = currentGameObject.activeInHierarchy ? ImGui.GetStyle().Colors[(int) ImGuiCol.Text] : ImGui.GetStyle().Colors[(int) ImGuiCol.TextDisabled];

		if (currentGameObject.isPrefab)
		{
			nameColor = currentGameObject.activeInHierarchy ? Color.SkyBlue.ToVector4() : new Color(135, 206, 235, 130).ToVector4();
		}

		ImGui.PushStyleColor(ImGuiCol.Text, nameColor);

		bool opened = ImGui.TreeNodeEx( /*$"[{currentGameObject.id}]" +*/ currentGameObject.name, flags);

		if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && false) // todo remove false
		{
			SceneNavigation.I.MoveToGameObject(Editor.I.GetSelectedGameObject());
		}

		if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
		{
			if (selectedGameObjectIndex != gameObjectIndexSelectedBefore)
			{
				SelectGameObject(Scene.I.gameObjects[gameObjectIndexSelectedBefore].id);
			}

			// select gameobject selected before
			string gameObjectID = currentGameObject.id.ToString();
			IntPtr stringPointer = Marshal.StringToHGlobalAnsi(gameObjectID);

			ImGui.SetDragDropPayload("GAMEOBJECT", stringPointer, (uint) (sizeof(char) * gameObjectID.Length));

			string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

			Marshal.FreeHGlobal(stringPointer);

			ImGui.EndDragDropSource();
		}

		if (ImGui.BeginDragDropTarget())
		{
			ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

			string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
			if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
			{
				GameObject foundGO = Scene.I.GetGameObject(int.Parse(payload));
				foundGO.transform.SetParent(currentGameObject.transform);
			}

			ImGui.EndDragDropTarget();
		}

		ImGui.PopStyleColor();

		if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
		{
			gameObjectIndexSelectedBefore = selectedGameObjectIndex;
			SelectGameObject(currentGameObject.id);
		}

		if (opened)
		{
			List<Transform> children = currentGameObject.transform.children;

			for (var childrenIndex = 0; childrenIndex < children.Count; childrenIndex++)
			{
				DrawGameObjectRow(children[childrenIndex].gameObject.indexInHierarchy, true);
				//ImGui.TreePop();
			}

			ImGui.TreePop();
		}
	}

	private enum MoveDirection
	{
		up,
		down
	}
}