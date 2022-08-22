namespace Scripts;

[Serializable]
public class Material
{
	public bool additive = false;
	public string path;
	public Shader shader;

	public int vao;
	public int vbo;

	public void SetShader(Shader _shader)
	{
		shader = _shader;

		shader.Load();
		BufferCache.CreateBufferForShader(this);
	}

	public void InitShader()
	{
		SetShader(shader);
	}
}