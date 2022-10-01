using System.Collections.Generic;
using System.IO;
using Engine.Tweening;

namespace Engine;

public class Scene
{
	public List<GameObject> gameObjects = new();

	private List<Renderer> renderQueue = new();

	public string scenePath = "";

	public Scene()
	{
		I = this;
	}

	public static Scene I { get; private set; }
	private Camera camera
	{
		get { return Camera.I; }
	}

	private void CreateDefaultObjects()
	{
		GameObject camGO = GameObject.Create(name: "Camera");
		camGO.AddComponent<Camera>();
		camGO.AddComponent<CameraController>();
		camGO.Awake();

		for (int i = 0; i < 1; i++)
		{
			GameObject go2 = GameObject.Create(name: "sprite " + i);
			go2.AddComponent<BoxRenderer>();
			go2.AddComponent<BoxShape>().size = new Vector2(400, 180);

			go2.Awake();
			go2.transform.position = new Vector2(0, 0);
			go2.transform.pivot = new Vector2(0.5f, 0.5f);
		}

		CreateTransformHandle();
	}

	private void SpawnTestSpriteRenderers()
	{
		GameObject parent = GameObject.Create(name: "parent");
		parent.Awake();
		for (int i = 0; i < 10000; i++)
		{
			GameObject go2 = GameObject.Create(name: "sprite " + i);
			go2.dynamicallyCreated = true;
			go2.AddComponent<SpriteRenderer>();
			go2.AddComponent<BoxShape>().size = new Vector2(400, 180);

			go2.Awake();
			go2.GetComponent<SpriteRenderer>().LoadTexture("2D/house.png");
			go2.transform.position = new Vector2(Rendom.Range(-1000, 1000), Rendom.Range(-1000, 1000));
			go2.transform.pivot = new Vector2(0.5f, 0.5f);
			go2.transform.SetParent(parent.transform);
		}
	}

	private void CreateTransformHandle()
	{
		GameObject transformHandleGameObject = GameObject.Create(_silent: true);
		Editor.I.transformHandle = transformHandleGameObject.AddComponent<TransformHandle>();
		transformHandleGameObject.dynamicallyCreated = true;
		transformHandleGameObject.alwaysUpdate = true;
		transformHandleGameObject.name = "Transform Handle";
		transformHandleGameObject.activeSelf = false;
		transformHandleGameObject.Awake();
	}

	public void Start()
	{
		Physics.Init();

		if (Serializer.lastScene != "" && File.Exists(Serializer.lastScene))
		{
			LoadScene(Serializer.lastScene);
		}
		else
		{
			CreateDefaultObjects();
		}
	}

	public void Update()
	{
		Time.Update();
		MouseInput.Update();
		TweenManager.I.Update();

		for (int i = 0; i < gameObjects.Count; i++)
		{
			gameObjects[i].indexInHierarchy = i;
			if (Global.GameRunning || gameObjects[i].alwaysUpdate)
			{
				gameObjects[i].Update();
				gameObjects[i].FixedUpdate();
			}
			else if (Global.GameRunning == false)
			{
				//gameObjects[i].EditorUpdate();
				gameObjects[i].Update();
			}
		}
	}

	public void OnComponentAdded(GameObject gameObject, Component component)
	{
		RenderQueueChanged();
	}

	public void RenderQueueChanged()
	{
		renderQueue = new List<Renderer>();
		for (int i = 0; i < gameObjects.Count; i++)
			if (gameObjects[i].GetComponent<Renderer>())
				//renderQueue.AddRange(gameObjects[i].GetComponents<Renderer>());
			{
				renderQueue.AddRange(gameObjects[i].GetComponents<Renderer>());
			}

		for (int i = 0; i < renderQueue.Count; i++) renderQueue[i].layerFromHierarchy = renderQueue[i].gameObject.indexInHierarchy * 0.00000000000000000000000000000001f;
		renderQueue.Sort();
	}

	public void Render()
	{
		GL.ClearColor(camera.color.ToOtherColor());
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		//BatchingManager.RenderAllBatchers();

		for (int i = 0; i < renderQueue.Count; i++)
		{
			if (renderQueue[i].enabled && renderQueue[i].awoken && renderQueue[i].gameObject.activeInHierarchy)
			{
				renderQueue[i].Render();
			}
		}
	}

	public SceneFile GetSceneFile()
	{
		SceneFile sf = new SceneFile();
		sf.Components = new List<Component>();
		sf.GameObjects = new List<GameObject>();
		for (int i = 0; i < gameObjects.Count; i++)
		{
			if (gameObjects[i].dynamicallyCreated)
			{
				continue;
			}

			sf.Components.AddRange(gameObjects[i].components);
			sf.GameObjects.Add(gameObjects[i]);
		}

		sf.gameObjectNextID = IDsManager.gameObjectNextID;
		return sf;
	}

	public GameObject FindGameObject(Type type)
	{
		foreach (GameObject gameObject in gameObjects)
		{
			Component bl = gameObject.GetComponent(type);
			if (bl != null)
			{
				return gameObject;
			}
		}

		return null;
	}

	public List<T> FindComponentsInScene<T>() where T : Component
	{
		List<T> components = new List<T>();
		foreach (GameObject gameObject in gameObjects)
		{
			T bl = gameObject.GetComponent<T>();
			if (bl != null)
			{
				components.Add(bl);
			}
		}

		return components;
	}

	public GameObject GetGameObject(int ID)
	{
		for (int i = 0; i < gameObjects.Count; i++)
			if (gameObjects[i].id == ID)
			{
				return gameObjects[i];
			}

		return null;
	}

	public void AddGameObjectToScene(GameObject gameObject)
	{
		gameObjects.Add(gameObject);

		RenderQueueChanged();
	}

	public bool LoadScene(string path = null)
	{
		Serializer.lastScene = path;

		//Add method to clean scene
		while (gameObjects.Count > 0) gameObjects[0].Destroy();
		gameObjects.Clear();

		//Physics.rigidbodies.Clear();

		gameObjects = new List<GameObject>();
		SceneFile sceneFile = Serializer.I.LoadGameObjects(path);

		Serializer.I.ConnectGameObjectsWithComponents(sceneFile);
		IDsManager.gameObjectNextID = sceneFile.gameObjectNextID + 1;

		Serializer.I.ConnectParentsAndChildren(sceneFile);

		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			for (int j = 0; j < sceneFile.GameObjects[i].components.Count; j++) sceneFile.GameObjects[i].components[j].gameObjectID = sceneFile.GameObjects[i].id;
			I.AddGameObjectToScene(sceneFile.GameObjects[i]);
		}

		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			sceneFile.GameObjects[i].LinkGameObjectFieldsInComponents();
			sceneFile.GameObjects[i].Awake();
		}


		CreateTransformHandle();

		scenePath = path;

		int lastSelectedGameObjectId = PersistentData.GetInt("lastSelectedGameObjectId", 0);
		if (Global.EditorAttached)
		{
			EditorWindow_Hierarchy.I.SelectGameObject(lastSelectedGameObjectId);
		}

		return true;
	}

	public void SaveScene(string path = null)
	{
		path = path ?? Serializer.lastScene;
		if (path.Length < 1)
		{
			path = Path.Combine("Assets", "scene1.scene");
		}

		Serializer.lastScene = path;
		Serializer.I.SaveGameObjects(GetSceneFile(), path);
	}

	public void CreateEmptySceneAndOpenIt(string path)
	{
		IDsManager.gameObjectNextID = 0;
		Serializer.lastScene = path;
		gameObjects = new List<GameObject>();
		CreateDefaultObjects();
		Serializer.I.SaveGameObjects(GetSceneFile(), path);
	}

	public void OnGameObjectDestroyed(GameObject gameObject)
	{
		if (gameObjects.Contains(gameObject))
		{
			gameObjects.Remove(gameObject);
		}

		RenderQueueChanged();
	}

	private void OnMouse3Clicked()
	{
	}

	private void OnMouse3Released()
	{
	}
}