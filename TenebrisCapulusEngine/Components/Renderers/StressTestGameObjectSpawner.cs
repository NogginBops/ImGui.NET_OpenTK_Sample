namespace Engine.Components.Renderers;

public class StressTestGameObjectSpawner : Component
{
	public int spawnCount = 1000;
	[XmlIgnore]
	public Action Spawn;
	[XmlIgnore]
	public Action Despawn;

	public GameObject go;

	public override void Awake()
	{
		Spawn += () =>
		{
			Serializer.I.SaveClipboardGameObject(go);

			for (int i = 0; i < spawnCount; i++)
			{
				Serializer.I.LoadClipboardGameObject();
			}
		};
		Despawn += () =>
		{
			for (int j = 0; j < Scene.I.gameObjects.Count; j++)
			{
				if (j > 10)
				{
					Scene.I.gameObjects[j].Destroy();
				}
			}
		};
	}
}