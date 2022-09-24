using System.IO;
using Engine.Components.Renderers;

namespace Scripts;

public class SpriteSheetRenderer : SpriteRenderer
{
	public int currentSpriteIndex;
	[Show]
	public Vector2 drawOffset = Vector2.Zero;

	private Vector2 spritesCount = new(1, 1);
	public Vector2 spriteSize;
	[Show]
	public float _zoomAmount;
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
		// CreateMaterial();
		//
		// //drawOffset = new Vector2(0, spriteSize.Y * spritesCount.Y - spriteSize.Y);
		//
		// //material = new Material(ShaderCache.spriteSheetRendererShader, BufferCache.spriteSheetRendererVAO);
		// if (texture == null)
		// {
		// 	texture = new Texture();
		// }
		// else
		// {
		// 	LoadTexture(texture.path);
		// }

		base.Awake();
	}

	public override void CreateMaterial()
	{
		if (material == null)
		{
			material = MaterialCache.GetMaterial("SpriteSheetRenderer");
		}

		base.CreateMaterial();
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
		if (Batched && false)
		{
			//BatchingManager.AddObjectToBatcher(texture.id, this);
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

		if (Batched && false)
		{
			var x = currentSpriteIndex % spritesCount.X;
			var y = (float) Math.Floor(currentSpriteIndex / spritesCount.X);

			drawOffset = new Vector2(x, y) * spriteSize * spritesCount;

			//BatchingManager.UpdateAttribsSpriteSheet(texture.id, gameObjectID, transform.position, new Vector2(GetComponent<BoxShape>().size.X * transform.scale.X, GetComponent<BoxShape>().size.Y * transform.scale.Y),
			//                                         color, drawOffset);
		}
		else
		{
			ShaderCache.UseShader(material.shader);
			material.shader.SetVector2("u_resolution", texture.size);
			material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);
			material.shader.SetColor("u_color", color.ToVector4());
			material.shader.SetVector2("u_scale", boxShape.size);


			var columnIndex = currentSpriteIndex % spritesCount.X;
			var rowIndex = (float) Math.Floor(currentSpriteIndex / spritesCount.X);

			drawOffset = new Vector2(columnIndex * spriteSize.X + spriteSize.X / 2, -rowIndex * spriteSize.Y - spriteSize.Y / 2);

			material.shader.SetVector2("offset", drawOffset);

			//_zoomAmount = texture.size.X/spriteSize.X*2;
			material.shader.SetFloat("zoomAmount", _zoomAmount);

			BufferCache.BindVAO(material.vao);

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