using System.Collections.Generic;

namespace Engine;

public struct SceneFile
{
	public List<GameObject> GameObjects;
	public List<Component> Components;
	public int gameObjectNextID;

	public static SceneFile CreateForOneGameObject(GameObject go)
	{
		SceneFile sceneFile = new SceneFile();
		sceneFile.GameObjects = new List<GameObject>();
		sceneFile.Components = new List<Component>();
		sceneFile.GameObjects.Add(go);
		sceneFile.Components.AddRange(go.components);

		for (int i = 0; i < go.transform.children.Count; i++)
		{
			sceneFile.GameObjects.Add(go.transform.children[i].gameObject);
			sceneFile.Components.AddRange(go.transform.children[i].gameObject.components);
		}

		//return new SceneFile() { GameObjects = new List<GameObject>() { go }, Components = go.components };
		return sceneFile;
	}
}