using System.Collections.Generic;

namespace Scripts;

public class Pool
{
	public Stack<GameObject> freeObjects = new();
	public GameObject model;
	public Stack<GameObject> usedObjects = new();

	private void AddNewObject()
	{
		GameObject gameObject = GameObject.Create(name: "Pooled object");
		for (int i = 0; i < model.components.Count; i++) gameObject.AddComponent(model.components[i].GetType());
		gameObject.Awake();
		freeObjects.Push(gameObject);
	}

	public GameObject Request()
	{
		if (freeObjects.Count == 0)
		{
			AddNewObject();
		}

		GameObject gameObject = freeObjects.Pop();
		gameObject.activeSelf = true;
		usedObjects.Push(gameObject);
		return gameObject;
	}

	public void Return(GameObject gameObject)
	{
		gameObject.activeSelf = false;
		freeObjects.Push(gameObject);
	}
}