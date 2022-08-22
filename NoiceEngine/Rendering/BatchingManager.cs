using System.Collections.Generic;
using System.Linq;

namespace Engine;

public class BatchingManager
{
	private static Dictionary<int, Batcher> batchers = new(); // textureID
	private static float[] attribsSkeleton_Sprite = {0, 0, 0, 0, 0, 0, 0, 0};
	private static float[] attribsSkeleton_SpriteSheet = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

	private static void CreateBatcherForTexture(Material material, Texture texture, Batcher.BatcherType batcherType)
	{
		Batcher batcher;
		if (batcherType == Batcher.BatcherType.Sprite)
		{
			batcher = new SpriteBatcher(10000, material, texture);
		}
		else
		{
			batcher = new SpriteSheetBatcher(10000, material, texture);
		}

		batchers.Add(texture.id, batcher);
	}

	public static void AddObjectToBatcher(int textureID, SpriteRenderer renderer, int instanceIndex = 0)
	{
		if (batchers.ContainsKey(textureID) == false)
		{
			CreateBatcherForTexture(renderer.material, renderer.texture, Batcher.BatcherType.Sprite);
		}

		batchers[textureID].AddGameObject(renderer.gameObjectID, instanceIndex);
	}

	/*public static void AddObjectToBatcher(int textureID, SpriteSheetRenderer renderer, int instanceIndex = 0)
	{
		if (batchers.ContainsKey(textureID) == false)
		{
			CreateBatcherForTexture(renderer.material, renderer.texture, Batcher.BatcherType.SpriteSheet);
		}

		batchers[textureID].AddGameObject(renderer.gameObjectID, instanceIndex);
	}*/

	public static void UpdateAttribs(int textureID, int gameObjectID, Vector2 position, Vector2 size, Color color, int instanceIndex = 0) //  use instanceIndex for particles-when we use single gameObject
	{
		if (batchers.ContainsKey(textureID) == false)
		{
			return;
		}

		attribsSkeleton_Sprite[0] = position.X;
		attribsSkeleton_Sprite[1] = position.Y;
		attribsSkeleton_Sprite[2] = size.X;
		attribsSkeleton_Sprite[3] = size.Y;
		attribsSkeleton_Sprite[4] = color.R / 255f;
		attribsSkeleton_Sprite[5] = color.G / 255f;
		attribsSkeleton_Sprite[6] = color.B / 255f;
		attribsSkeleton_Sprite[7] = color.A / 255f;
		/*attribsSkeleton = new float[]
		                  {
			                  position.X, position.Y, size.X , size.Y, color.R/255f,color.G/255f,color.B/255f,color.A
		                  };*/
		//float[] attribs = new float[] {0, 0, 2000, 2000};
		batchers[textureID].SetAttribs(gameObjectID, attribsSkeleton_Sprite, instanceIndex);
	}

	public static void RemoveAttribs(int textureID, int gameObjectID, int instanceIndex = 0)
	{
		if (batchers.ContainsKey(textureID) == false)
		{
			return;
		}

		attribsSkeleton_Sprite[0] = 0;
		attribsSkeleton_Sprite[1] = 0;
		attribsSkeleton_Sprite[2] = 0;
		attribsSkeleton_Sprite[3] = 0;
		attribsSkeleton_Sprite[4] = 0;
		attribsSkeleton_Sprite[5] = 0;
		attribsSkeleton_Sprite[6] = 0;
		attribsSkeleton_Sprite[7] = 0;
		/*attribsSkeleton = new float[]
		                  {
			                  position.X, position.Y, size.X , size.Y, color.R/255f,color.G/255f,color.B/255f,color.A
		                  };*/
		//float[] attribs = new float[] {0, 0, 2000, 2000};
		batchers[textureID].SetAttribs(gameObjectID, attribsSkeleton_Sprite, instanceIndex);
	}

	public static void
		UpdateAttribsSpriteSheet(int textureID, int gameObjectID, Vector2 position, Vector2 size, Color color, Vector2 drawOffset, int instanceIndex = 0) //  use instanceIndex for particles-when we use single gameObject
	{
		if (batchers.ContainsKey(textureID) == false)
		{
			return;
		}

		attribsSkeleton_SpriteSheet[0] = position.X;
		attribsSkeleton_SpriteSheet[1] = position.Y;
		attribsSkeleton_SpriteSheet[2] = size.X;
		attribsSkeleton_SpriteSheet[3] = size.Y;
		attribsSkeleton_SpriteSheet[4] = color.R / 255f;
		attribsSkeleton_SpriteSheet[5] = color.G / 255f;
		attribsSkeleton_SpriteSheet[6] = color.B / 255f;
		attribsSkeleton_SpriteSheet[7] = color.A / 255f;
		attribsSkeleton_SpriteSheet[8] = drawOffset.X;
		attribsSkeleton_SpriteSheet[9] = drawOffset.Y;

		/*attribsSkeleton = new float[]
		                  {
			                  position.X, position.Y, size.X , size.Y, color.R/255f,color.G/255f,color.B/255f,color.A
		                  };*/
		//float[] attribs = new float[] {0, 0, 2000, 2000};
		batchers[textureID].SetAttribs(gameObjectID, attribsSkeleton_SpriteSheet, instanceIndex);
	}

	public static void RenderAllBatchers()
	{
		for (int i = 0; i < batchers.Count; i++) batchers.ElementAt(i).Value.Render();
	}
}