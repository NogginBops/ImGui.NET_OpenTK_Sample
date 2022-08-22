/*using System.IO;

namespace Scripts;

public class SpriteSheetRenderer : SpriteRenderer
{
	public int currentSpriteIndex;
	private Vector2 drawOffset = Vector2.Zero;
	public Vector2 drawOffset_TEST = Vector2.Zero;

	private Vector2 spritesCount = new(1, 1);
	public Vector2 spriteSize;
	[Hide] public override bool Batched { get; set; } = true;
	public Vector2 SpritesCount
	{
		get { return spritesCount; }
		set
		{
			spritesCount = value;
			if (texture != null)
			{
				spriteSize = new Vector2(texture.size.X / SpritesCount.X, texture.size.Y / SpritesCount.Y);
			}
		}
	}

	public override void Awake()
	{
		drawOffset = new Vector2(0, spriteSize.Y * spritesCount.Y - spriteSize.Y);

		//material = new Material(ShaderCache.spriteSheetRendererShader, BufferCache.spriteSheetRendererVAO);
		if (texture == null)
		{
			texture = new Texture();
		}
		else
		{
			LoadTexture(texture.path);
		}

		base.Awake();
	}

	internal override void UpdateBoxShapeSize()
	{
	}

	public override void OnNewComponentAdded(Component comp)
	{
	}

	public override void LoadTexture(string _texturePath)
	{
		if (_texturePath.Contains("Assets") == false)
		{
			_texturePath = Path.Combine("Assets", _texturePath);
		}

		if (File.Exists(_texturePath) == false)
		{
			return;
		}

		texture.Load(_texturePath);

		UpdateBoxShapeSize();
		if (Batched)
		{
			BatchingManager.AddObjectToBatcher(texture.id, this);
		}
	}

	public override void Render()
	{
		if (onScreen == false)
		{
			return;
		}

		if (boxShape == null)
		{
			return;
		}

		if (texture.loaded == false)
		{
			return;
		}

		if (Batched)
		{
			var x = currentSpriteIndex % spritesCount.X;
			var y = (float) Math.Floor(currentSpriteIndex / spritesCount.X);

			drawOffset = new Vector2(x, y) * spriteSize * spritesCount;

			BatchingManager.UpdateAttribsSpriteSheet(texture.id, gameObjectID, transform.position, new Vector2(GetComponent<BoxShape>().size.X * transform.scale.X, GetComponent<BoxShape>().size.Y * transform.scale.Y),
			                                         color, drawOffset);
		}
		else
		{
			ShaderCache.UseShader(ShaderCache.spriteSheetRendererShader);
			ShaderCache.spriteSheetRendererShader.SetVector2("u_resolution", texture.size);
			ShaderCache.spriteSheetRendererShader.SetMatrix4x4("u_mvp", LatestModelViewProjection);
			ShaderCache.spriteSheetRendererShader.SetColor("u_color", color.ToVector4());
			ShaderCache.spriteSheetRendererShader.SetVector2("u_scale", boxShape.size);


			var x = currentSpriteIndex % spritesCount.X;
			var y = (float) Math.Floor(currentSpriteIndex / spritesCount.X);

			drawOffset = new Vector2(x, y) * spriteSize * spritesCount;

			ShaderCache.spriteSheetRendererShader.SetVector2("u_offset", drawOffset);

			BufferCache.BindVAO(BufferCache.spriteSheetRendererVAO);

			if (material.additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}

			TextureCache.BindTexture(texture.id);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			//BufferCache.BindVAO(0);
			//GL.Disable(EnableCap.Blend);

			Debug.CountStat("Draw Calls", 1);
		}
	}
}
/*public override void OnTextureLoaded(Texture2D _texture, string _path)
{
	SpriteSize = new Vector2(_texture.Width / SpritesCount.X, _texture.Height / SpritesCount.Y);

	base.OnTextureLoaded(_texture, _path);
}#1#*/

