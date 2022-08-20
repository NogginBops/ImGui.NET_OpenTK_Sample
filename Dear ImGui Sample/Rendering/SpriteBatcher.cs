using System.Collections.Generic;
using System.Numerics;

namespace Engine;

public class SpriteBatcher : Batcher
{
	public SpriteBatcher(int size, Material material, Texture texture) : base(size, material, texture)
	{
		vertexAttribSize = 8;
	}

	public override void CreateBuffers()
	{
		vao = GL.GenVertexArray();
		vbo = GL.GenBuffer();

		BufferCache.BindVAO(vao);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

		List<float> verticesList = new List<float>();

		// 100 limit for now, but it can be dynamic too
		for (int i = 0; i < size; i++)
			verticesList.AddRange(new[]
			                      {
				                      -0.5f, -0.5f, 0, 0,
				                      0.5f, -0.5f, 1, 0,
				                      -0.5f, 0.5f, 0, 1,

				                      -0.5f, 0.5f, 0, 1,
				                      0.5f, -0.5f, 1, 0,
				                      0.5f, 0.5f, 1, 1
			                      });

		float[] vertices = verticesList.ToArray();

		GL.NamedBufferStorage(
		                      vbo,
		                      sizeof(float) * vertices.Length,
		                      vertices,
		                      BufferStorageFlags.MapWriteBit);

		// ATTRIB: vertex position -   2 floats
		GL.VertexArrayAttribBinding(vao, 0, 0);
		GL.EnableVertexArrayAttrib(vao, 0);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           0, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           false,
		                           0); // relative offset, first item

		// ATTRIB: texture coord -  2 floats
		GL.VertexArrayAttribBinding(vao, 1, 0);
		GL.EnableVertexArrayAttrib(vao, 1);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           1, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           true,
		                           8); // relative offset, first item

		GL.VertexArrayVertexBuffer(vao, 0, vbo, new IntPtr(0), sizeof(float) * 4);
	}

	public override void Render()
	{
		if (material == null)
		{
			return;
		}

		if (texture == null)
		{
			return;
		}

		if (vao == -1)
		{
			CreateBuffers();
		}

		bool createdBufferThisFrame = false;
		if (vbo_attribs == -1)
		{
			vbo_attribs = GL.GenBuffer();
			createdBufferThisFrame = true;
		}

		BufferCache.BindVAO(vao);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_attribs);


		float[] attribsArray = attribs.ToArray();


		GL.NamedBufferData(
		                   vbo_attribs,
		                   sizeof(float) * attribsArray.Length,
		                   attribsArray,
		                   BufferUsageHint.StreamCopy);
		currentBufferUploadedSize = attribsArray.Length;


		// ATTRIB: vertex position -   2 floats
		GL.VertexArrayAttribBinding(vao, 2, 1);
		GL.EnableVertexArrayAttrib(vao, 2);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           2, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           false,
		                           0); // relative offset, first item

		// ATTRIB: size -   2 floats
		GL.VertexArrayAttribBinding(vao, 3, 1);
		GL.EnableVertexArrayAttrib(vao, 3);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           3, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           false,
		                           8); // relative offset, first item


		// ATTRIB: color -   4 floats
		GL.VertexArrayAttribBinding(vao, 4, 1);
		GL.EnableVertexArrayAttrib(vao, 4);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           4, // attribute index, from the shader location = 4
		                           4, // size of attribute
		                           VertexAttribType.Float, // uint representation of a color
		                           true,
		                           16); // relative offset, first item


		GL.VertexArrayVertexBuffer(vao, 1, vbo_attribs, IntPtr.Zero, sizeof(float) * 8);


		ShaderCache.UseShader(material.shader);
		material.shader.SetVector2("u_resolution", texture.size);
		material.shader.SetMatrix4x4("u_mvp", Matrix4x4.Identity * Camera.I.viewMatrix * Camera.I.projectionMatrix);
		material.shader.SetColor("u_color", Color.White.ToVector4());

		BufferCache.BindVAO(vao);

		if (material.additive)
		{
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
		}
		else
		{
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		TextureCache.BindTexture(texture.id);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * size);


		Debug.CountStat("Draw Calls", 1);
	}
}