using System.Collections.Generic;
using System.Xml.Serialization;

namespace Scripts;

public class InspectorObjectReferencesTest : Component
{
	[Show] public List<GameObject> gos = new List<GameObject>();

	[XmlIgnore] public Action AddGameObjectToList;

	private Vector2 snakePos;
	private List<Vector2> mouseCachedPositions = new List<Vector2>();

	public override void Awake()
	{

		//gos = new List<GameObject>();
		AddGameObjectToList = () =>
		{
			GameObject go = GameObject.Create(name: "testGO" + Scene.I.gameObjects.Count);
			go.transform.pivot = new Vector3(0.5f, 0.5f, 0.5f);
			BoxShape boxShape = go.AddComponent<BoxShape>();
			boxShape.size = new Vector2(100, 100);
			BoxRenderer boxRenderer = go.AddComponent<BoxRenderer>();
			boxRenderer.Layer = 100 - gos.Count;
			boxRenderer.color = Extensions.ColorFromHSV(gos.Count * 33f, 0.7f, 0.8f);
			go.Awake();
			gos.Add(go);
		};
		base.Awake();
	}

	public override void Update()
	{
		base.Update();
	}
}