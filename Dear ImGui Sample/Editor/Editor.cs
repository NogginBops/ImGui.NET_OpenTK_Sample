using System.Collections.Generic;
using ImGuiNET;

namespace Engine;

public class Editor
{
	public static Vector2 sceneViewPosition = new(0, 0);
	public static Vector2 sceneViewSize = new(0, 0);
	//private ImGuiRenderer _imGuiRenderer;
	private EditorWindow[] editorWindows;

	public TransformHandle transformHandle;

	//public static Vector2 ScreenToWorld(Vector2 screenPosition)
	//{
	//	return (screenPosition - gameViewPosition) * Camera.I.cameraSize + Camera.I.transform.position;
	//}
	public Editor()
	{
		I = this;
	}

	public static Editor I { get; private set; }

	public void Init()
	{
		ImGui.GetStyle().WindowRounding = 0;
		ImGui.GetStyle().WindowBorderSize = 0.2f;
		//ImGui.GetStyle().WindowPadding = new Vector2(0,0;

		ImGuiStylePtr style = ImGui.GetStyle();
		RangeAccessor<System.Numerics.Vector4> colors = style.Colors;
		int theme = 1;
		if (theme == 0)
		{
			colors[(int) ImGuiCol.Text] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
			colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.500f, 0.500f, 0.500f, 1.000f);
			colors[(int) ImGuiCol.WindowBg] = new Vector4(0.180f, 0.180f, 0.180f, 1.000f);
			colors[(int) ImGuiCol.ChildBg] = new Vector4(0.280f, 0.280f, 0.280f, 0.000f);
			colors[(int) ImGuiCol.PopupBg] = new Vector4(0.313f, 0.313f, 0.313f, 1.000f);
			colors[(int) ImGuiCol.Border] = new Vector4(0.266f, 0.266f, 0.266f, 1.000f);
			colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.000f, 0.000f, 0.000f, 0.000f);
			colors[(int) ImGuiCol.FrameBg] = new Vector4(0.160f, 0.160f, 0.160f, 1.000f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.200f, 0.200f, 0.200f, 1.000f);
			colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.280f, 0.280f, 0.280f, 1.000f);
			colors[(int) ImGuiCol.TitleBg] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.160f, 0.160f, 0.160f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.277f, 0.277f, 0.277f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.300f, 0.300f, 0.300f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.CheckMark] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
			colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.391f, 0.391f, 0.391f, 1.000f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.Button] = new Vector4(1.000f, 1.000f, 1.000f, 0.000f);
			colors[(int) ImGuiCol.ButtonHovered] = new Vector4(1.000f, 1.000f, 1.000f, 0.156f);
			colors[(int) ImGuiCol.ButtonActive] = new Vector4(1.000f, 1.000f, 1.000f, 0.391f);
			colors[(int) ImGuiCol.Header] = new Vector4(0.313f, 0.313f, 0.313f, 1.000f);
			colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.391f, 0.391f, 0.391f, 1.000f);
			colors[(int) ImGuiCol.SeparatorActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.ResizeGrip] = new Vector4(1.000f, 1.000f, 1.000f, 0.250f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(1.000f, 1.000f, 1.000f, 0.670f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.Tab] = new Vector4(0.098f, 0.098f, 0.098f, 1.000f);
			colors[(int) ImGuiCol.TabHovered] = new Vector4(0.352f, 0.352f, 0.352f, 1.000f);
			colors[(int) ImGuiCol.TabActive] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.098f, 0.098f, 0.098f, 1.000f);
			colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.DockingPreview] = new Vector4(1.000f, 0.391f, 0.000f, 0.781f);
			colors[(int) ImGuiCol.DockingEmptyBg] = new Vector4(0.180f, 0.180f, 0.180f, 1.000f);
			colors[(int) ImGuiCol.PlotLines] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.586f, 0.586f, 0.586f, 1.000f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(1.000f, 1.000f, 1.000f, 0.156f);
			colors[(int) ImGuiCol.DragDropTarget] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavHighlight] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.000f, 0.000f, 0.000f, 0.586f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.000f, 0.000f, 0.000f, 0.586f);
			colors[(int) ImGuiCol.Separator] = colors[(int) ImGuiCol.Border];
		}
		else if (theme == 1)
		{
			colors[(int) ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
			colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
			colors[(int) ImGuiCol.WindowBg] = new Vector4(0.13f, 0.13f, 0.13f, 1.000f);
			colors[(int) ImGuiCol.ChildBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.PopupBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
			colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.3f, 0.3f, 0.3f, 0f);
			colors[(int) ImGuiCol.FrameBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
			colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
			colors[(int) ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
			colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
			colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
			colors[(int) ImGuiCol.CheckMark] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.08f, 0.50f, 0.72f, 1.00f);
			colors[(int) ImGuiCol.Button] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
			colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
			colors[(int) ImGuiCol.Header] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
			colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.41f, 0.42f, 0.44f, 1.00f);
			colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
			colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.29f, 0.30f, 0.31f, 0.67f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
			colors[(int) ImGuiCol.Tab] = new Vector4(0.08f, 0.08f, 0.09f, 0.83f);
			colors[(int) ImGuiCol.TabHovered] = new Vector4(0.33f, 0.34f, 0.36f, 0.83f);
			colors[(int) ImGuiCol.TabActive] = new Vector4(0.23f, 0.23f, 0.24f, 1.00f);
			colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
			colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.DockingPreview] = new Vector4(0.26f, 0.59f, 0.98f, 0.70f);
			colors[(int) ImGuiCol.DockingEmptyBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
			colors[(int) ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
			colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
			colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
			colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
			colors[(int) ImGuiCol.Separator] = colors[(int) ImGuiCol.Border];
		}

		if (Global.EditorAttached)
		{
			editorWindows = new EditorWindow[]
			                {
				                new EditorWindow_Hierarchy(),
				                new EditorWindow_Inspector(),
				                new EditorWindow_Browser(),
				                new EditorWindow_Console(),
				                new EditorWindow_Profiler(),
				                new EditorWindow_SceneView()
			                };
		}
		else
		{
			editorWindows = new EditorWindow[]
			                {
				                new EditorWindow_SceneView()
			                };
		}

		for (int i = 0; i < editorWindows.Length; i++) editorWindows[i].Init();

		if (Global.EditorAttached)
		{
			EditorWindow_Hierarchy.I.GameObjectSelected += OnGameObjectSelected;
			EditorWindow_Hierarchy.I.GameObjectSelected += EditorWindow_Inspector.I.OnGameObjectSelected;
		}
	}

	public void Update()
	{
		for (int i = 0; i < editorWindows.Length; i++) editorWindows[i].Update();

		if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.IsKeyDown(Keys.S))
		{
			if (Global.GameRunning == false)
			{
				Scene.I.SaveScene();
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.IsKeyDown(Keys.R))
		{
			if (Global.GameRunning == false)
			{
				Scene.I.LoadScene(Serializer.lastScene);
			}
		}
	}

	public void Draw()
	{
		if (Global.EditorAttached)
		{
			for (int i = 0; i < editorWindows.Length; i++)
				editorWindows[i].Draw();
		}
		else
		{
			EditorWindow_SceneView.I.Draw();
		}
	}

	public void SelectGameObject(GameObject go)
	{
		if (go != null)
		{
			for (int i = 0; i < Scene.I.gameObjects.Count; i++)
				if (Scene.I.gameObjects[i].id != go.id)
				{
					Scene.I.gameObjects[i].selected = false;
				}

			go.selected = true;
		}

		if (go != Camera.I.gameObject && go != transformHandle.gameObject && go != null)
		{
			transformHandle.SelectObject(go);
			PersistentData.Set("lastSelectedGameObjectId", go.id);
		}
		else
		{
			transformHandle.SelectObject(null);
		}
	}

	private void OnGameObjectSelected(int id)
	{
		if (Global.EditorAttached == false)
		{
			id = -1;
		}

		if (id == -1)
		{
			SelectGameObject(null);
		}
		else
		{
			if (GetGameObjectIndexInHierarchy(id) == -1)
			{
				return;
			}

			SelectGameObject(Scene.I.gameObjects[GetGameObjectIndexInHierarchy(id)]);
		}
	}

	public int GetGameObjectIndexInHierarchy(int id)
	{
		for (int i = 0; i < Scene.I.gameObjects.Count; i++)
			if (Scene.I.gameObjects[i].id == id)
			{
				return i;
			}

		return -1;
	}

	public List<GameObject> GetSelectedGameObjects()
	{
		List<GameObject> selectedGameObjects = new List<GameObject>();
		for (int i = 0; i < Scene.I.gameObjects.Count; i++)
			if (Scene.I.gameObjects[i].selected)
			{
				selectedGameObjects.Add(Scene.I.gameObjects[i]);
			}

		return selectedGameObjects;
	}

	public GameObject GetSelectedGameObject()
	{
		for (int i = 0; i < Scene.I.gameObjects.Count; i++)
			if (Scene.I.gameObjects[i].selected)
			{
				return Scene.I.gameObjects[i];
			}

		return null;
	}
}