using System.IO;

namespace Tofu3D;

public static class ModelAssetManager
{
	public static Model LoadModel(string modelPath)
	{
		using (StreamReader sr = new StreamReader(modelPath))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Model));
			Model model = (Model) xmlSerializer.Deserialize(sr);
			model.path = modelPath;
			/*if (mat.shader != null)
			{
				mat.SetShader(mat.shader);
			}*/

			return model;
		}
	}

	public static void SaveModel(Model model)
	{
		using (StreamWriter sw = new StreamWriter(model.path))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Model));

			xmlSerializer.Serialize(sw, model);
		}
	}
}