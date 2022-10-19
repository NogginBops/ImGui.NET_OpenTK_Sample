namespace Tofu3D;

[Serializable]
public class Texture
{
	public int id;
	public bool loaded;
	public string path = "";
	public Vector2 size;

	public void Load(string _path, bool flipX = true, bool smooth = false)
	{
		path = _path;
		Texture loadedTexture = TextureCache.GetTexture(_path, flipX,smooth);

		id = loadedTexture.id;
		size = loadedTexture.size;

		loaded = true;
	}

	public void Delete()
	{
		TextureCache.DeleteTexture(path);
	}
}