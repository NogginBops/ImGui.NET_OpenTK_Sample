using System.Collections.Generic;
using System.IO;

namespace Engine.Components.Renderers;

public class MaterialCache
{
	private static List<Material> loadedMaterials = new List<Material>();

	public static Material GetMaterial(string name)
	{
		if (name.Contains(".mat") == false)
		{
			name += ".mat";
		}
		for (int i = 0; i < loadedMaterials.Count; i++)
		{
			if (loadedMaterials[i].path.Contains(name))
			{
				return loadedMaterials[i];
			}
		}

		loadedMaterials.Add(MaterialAssetManager.LoadMaterial(Path.Combine(Folders.Materials, name)));

		for (int i = 0; i < loadedMaterials.Count; i++)
		{
			if (loadedMaterials[i].path.Contains(name))
			{
				return loadedMaterials[i];
			}
		}

		return null;
	}
}