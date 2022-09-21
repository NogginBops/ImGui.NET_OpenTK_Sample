using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Engine;

public static class TextureCache
{
	private static Dictionary<int, Texture> cachedTextures = new();
	private static int textureInUse = -1;

	private static Texture LoadAndCreateTexture(string texturePath, bool flipX = true)
	{
		int id = GL.GenTexture();
		BindTexture(id);

		Image<Rgba32> image = Image.Load<Rgba32>(texturePath);
		if (flipX)
		{
			image.Mutate(x => x.Flip(FlipMode.Vertical));
		}

		List<byte> pixels = new List<byte>(4 * image.Width * image.Height);

		for (int y = 0; y < image.Height; y++)
		{
			Span<Rgba32> row = image.GetPixelRowSpan(y);
			for (int x = 0; x < image.Width; x++)
			{
				pixels.Add(row[x].R);
				pixels.Add(row[x].G);
				pixels.Add(row[x].B);
				pixels.Add(row[x].A);
			}
		}

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

		Texture texture = new Texture();
		texture.id = id;
		texture.size = new Vector2(image.Width, image.Height);
		texture.loaded = true;
		texture.path = texturePath;

		cachedTextures.Add(GetHash(texturePath), texture);
		return texture;
	}

	public static Texture GetTexture(string texturePath, bool flipX = true)
	{
		if (cachedTextures.ContainsKey(GetHash(texturePath)) == false)
		{
			return LoadAndCreateTexture(texturePath, flipX);
		}

		return cachedTextures[GetHash(texturePath)];
	}

	public static void DeleteTexture(string texturePath)
	{
		if (cachedTextures.ContainsKey(GetHash(texturePath)))
		{
			GL.DeleteTexture(cachedTextures[GetHash(texturePath)].id);

			cachedTextures.Remove(GetHash(texturePath));
		}
	}

	public static int GetHash(string texturePath)
	{
		return texturePath.GetHashCode();
	}

	public static void BindTexture(int id)
	{
		if (id == textureInUse)
		{
			return;
		}

		textureInUse = id;
		GL.BindTexture(TextureTarget.Texture2D, id);
	}
}