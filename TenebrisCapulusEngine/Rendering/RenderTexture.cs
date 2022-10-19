using System.IO;

namespace Tofu3D;

public class RenderTexture
{
	public int colorAttachment;
	public int id;

	public Material renderTextureMaterial;

	public RenderTexture(Vector2 size)
	{
		//GL.DeleteFramebuffers(1, ref id);
		CreateMaterial();
		Invalidate(size);
	}

	private void CreateMaterial()
	{
		renderTextureMaterial = new Material();
		Shader shader = new(Path.Combine(Folders.Shaders, "RenderTexture.glsl"));
		renderTextureMaterial.SetShader(shader);
	}

	public void Invalidate(Vector2 size)
	{
		id = GL.GenFramebuffer();

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

		colorAttachment = GL.GenTexture();
		//GL.CreateTextures(TextureTarget.Texture2D, 1, out colorAttachment);
		GL.BindTexture(TextureTarget.Texture2D, colorAttachment);
		//TextureCache.BindTexture(colorAttachment);

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, (int) size.X, (int) size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr) null);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Linear);

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorAttachment, 0);

		if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
		{
			Debug.Log("RENDER TEXTURE ERROR");
		}

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public void Bind()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
	}

	public void Unbind()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public void Render(int targetTexture, float sampleSize = 1)
	{
		if (renderTextureMaterial == null)
		{
			return;
		}

		//return;
		ShaderCache.UseShader(renderTextureMaterial.shader);
		// renderTextureMaterial.shader.SetVector2("u_resolution", Camera.I.size);
		//	renderTextureMaterial.shader.SetMatrix4x4("u_mvp", GetModelViewProjection(sampleSize));

		ShaderCache.BindVAO(renderTextureMaterial.vao);
		GL.Enable(EnableCap.Blend);


		TextureCache.BindTexture(targetTexture);

		//GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		ShaderCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
	}

	/*public void RenderSnow(int targetTexture)
	{
		ShaderCache.UseShader(ShaderCache.snowShader);
		ShaderCache.snowShader.SetFloat("time", Time.elapsedTime * 0.08f);
		ShaderCache.snowShader.SetMatrix4x4("u_mvp", GetModelViewProjection(1));
		ShaderCache.snowShader.SetVector2("u_resolution", Camera.I.size);

		BufferCache.BindVAO(BufferCache.snowVAO);
		GL.Enable(EnableCap.Blend);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Lequal);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.BlendFunc(BlendingFactor.SrcColor, BlendingFactor.One);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		BufferCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
		GL.Disable(EnableCap.DepthTest);
	}*/

	/*public void RenderWithPostProcess(int targetTexture)
	{
		ShaderCache.UseShader(ShaderCache.renderTexturePostProcessShader);
		ShaderCache.renderTexturePostProcessShader.SetVector2("u_resolution", Camera.I.size);
		ShaderCache.renderTexturePostProcessShader.SetMatrix4x4("u_mvp", GetModelViewProjection(1));

		BufferCache.BindVAO(BufferCache.renderTexturePostProcessVAO);
		GL.Enable(EnableCap.Blend);


		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		BufferCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
	}*/

	/*public void RenderBloom(int targetTexture, float sampleSize = 1)
	{
		ShaderCache.UseShader(ShaderCache.renderTextureBloomShader);
		ShaderCache.renderTextureBloomShader.SetVector2("u_resolution", Camera.I.size);
		ShaderCache.renderTextureBloomShader.SetMatrix4x4("u_mvp", GetModelViewProjection(sampleSize));

		BufferCache.BindVAO(BufferCache.renderTextureBloomVAO);
		GL.Enable(EnableCap.Blend);


		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstColor);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		BufferCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
	}*/

	public Matrix4x4 GetModelViewProjection(float sampleSize)
	{
		return Matrix4x4.CreateScale(2 * sampleSize, 2 * sampleSize, 1);
	}
}