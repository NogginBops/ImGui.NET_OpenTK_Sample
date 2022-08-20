using System.IO;
using System.Xml.Serialization;

namespace Engine;

public static class MaterialAssetManager
{
	public static void CreateDefaultMaterials()
	{
		{
			Material boxMaterial = new();

			Shader boxShader = new(Path.Combine(Folders.Shaders, "BoxRenderer.glsl"));
			boxMaterial.SetShader(boxShader);
			using (StreamWriter sw = new StreamWriter(Path.Combine(Folders.Materials, "BoxMaterial.mat")))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Material));

				xmlSerializer.Serialize(sw, boxMaterial);
			}
		}
		{
			Material renderTextureMaterial = new();

			Shader renderTextureShader = new(Path.Combine(Folders.Shaders, "RenderTexture.glsl"));
			renderTextureMaterial.SetShader(renderTextureShader);
			using (StreamWriter sw = new StreamWriter(Path.Combine(Folders.Materials, "RenderTexture.mat")))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Material));

				xmlSerializer.Serialize(sw, renderTextureMaterial);
			}
		}
		{
			Material renderTextureMaterial = new();

			Shader renderTextureShader = new(Path.Combine(Folders.Shaders, "SpriteRenderer.glsl"));
			renderTextureMaterial.SetShader(renderTextureShader);
			using (StreamWriter sw = new StreamWriter(Path.Combine(Folders.Materials, "SpriteRenderer.mat")))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Material));

				xmlSerializer.Serialize(sw, renderTextureMaterial);
			}
		}
	}

	public static Material LoadMaterial(string materialPath)
	{
		using (StreamReader sr = new StreamReader(materialPath))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Material));
			Material mat = (Material) xmlSerializer.Deserialize(sr);
			mat.path = materialPath;
			if (mat.shader != null)
			{
				mat.SetShader(mat.shader);
			}

			return mat;
		}
	}

	public static void SaveMaterial(Material material)
	{
		using (StreamWriter sw = new StreamWriter(material.path))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Material));

			xmlSerializer.Serialize(sw, material);
		}
	}
}