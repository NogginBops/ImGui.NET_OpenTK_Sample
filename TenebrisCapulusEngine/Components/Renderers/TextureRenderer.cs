using System.IO;

public class TextureRenderer : Renderer
{
	public Texture texture;
	[XmlIgnore]
	public Action SetNativeSize;

	public virtual void LoadTexture(string _texturePath)
	{
		if (_texturePath.Contains("Assets") == false)
		{
			_texturePath = Path.Combine("Assets", _texturePath);
		}

		if (File.Exists(_texturePath) == false)
		{
			return;
		}

		if (texture == null)
		{
			texture = new Texture();
		}

		texture.Load(_texturePath);
	}
}