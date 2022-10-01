using System.Collections.Generic;
using System.IO;

namespace Engine.Components.Renderers;

public class ModelCache
{
	private static List<Model> loadedModels = new List<Model>();

	public static Model GetModel(string name)
	{
		if (name.Contains(".model") == false)
		{
			name += ".model";
		}

		for (int i = 0; i < loadedModels.Count; i++)
		{
			if (loadedModels[i].path.Contains(name))
			{
				return loadedModels[i];
			}
		}

		loadedModels.Add(ModelAssetManager.LoadModel(Path.Combine(Folders.Models, name)));

		for (int i = 0; i < loadedModels.Count; i++)
		{
			if (loadedModels[i].path.Contains(name))
			{
				return loadedModels[i];
			}
		}

		return null;
	}
}