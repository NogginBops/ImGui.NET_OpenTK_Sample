using System.Collections.Generic;

namespace Engine;

public abstract class Batcher
{
	public enum BatcherType
	{
		Sprite,
		SpriteSheet
	}

	internal List<float> attribs = new();
	internal int currentBufferUploadedSize = 0;
	public Material material;
	internal Dictionary<int, int> rendererLocationsInAttribs = new(); // key:renderer ID, value:index in attribs list

	internal int size;
	public Texture texture;
	internal int vao = -1;
	internal int vbo = -1;
	internal int vbo_attribs = -1;
	internal int vertexAttribSize;

	public Batcher(int size, Material material, Texture texture)
	{
		this.size = size;
		this.material = material;
		this.texture = texture;
	}

	public abstract void CreateBuffers();

	public abstract void Render();

	public void AddGameObject(int gameObjectID, int instanceIndex = 0)
	{
		int index = gameObjectID;
		if (instanceIndex != 0)
		{
			index = -gameObjectID - instanceIndex * vertexAttribSize;
		}

		if (rendererLocationsInAttribs.ContainsKey(index))
		{
			return;
		}

		rendererLocationsInAttribs.Add(index, attribs.Count);

		float[] _att = new float[vertexAttribSize];

		for (int i = 0; i < 6; i++) attribs.AddRange(_att);
	}

	public void SetAttribs(int gameObjectID, float[] _attribs, int instanceIndex = 0)
	{
		int index = gameObjectID;
		if (instanceIndex != 0)
		{
			index = -gameObjectID - instanceIndex * vertexAttribSize;
		}

		for (int i = 0; i < 6; i++)
		for (int j = 0; j < vertexAttribSize; j++)
			attribs[rendererLocationsInAttribs[index] + i * vertexAttribSize + j] = _attribs[j];
	}
}