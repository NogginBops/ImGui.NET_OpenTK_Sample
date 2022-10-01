using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImGuiNET;

namespace Engine;

public static class BuffersGenerator
{
	public static void CreateBufferForShader(Material material)
	{
		if (material.shader.bufferType == BufferType.BOX)
		{
			CreateBoxRendererBuffers(ref material.vao);
			//CreateBoxRendererBuffers(ref material.vao, ref material.vbo);
		}

		if (material.shader.bufferType == BufferType.RENDERTEXTURE)
		{
			CreateRenderTextureBuffers(ref material.vao);
		}

		if (material.shader.bufferType == BufferType.SPRITE)
		{
			CreateSpriteRendererBuffers(ref material.vao);
			//CreateSpriteRendererBuffersz(ref material.vao, ref material.vbo);
		}
		if (material.shader.bufferType == BufferType.MODEL)
		{
			CreateModelBuffers(ref material.vao);
			//CreateSpriteRendererBuffersz(ref material.vao, ref material.vbo);
		}

		/*if (material.shader.bufferType == BufferType.GRADIENT)
		{
			CreateBoxRendererBuffers(ref material.vao, ref material.vbo);
		}*/
	}

	private static void CreateRenderTextureBuffers(ref int vao)
	{
		int vbo = GL.GenBuffer();

		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

		float[] vertices =
		{
			-0.5f, -0.5f, 0, 0,
			0.5f, -0.5f, 1, 0,
			-0.5f, 0.5f, 0, 1,
			-0.5f, 0.5f, 0, 1,
			0.5f, -0.5f, 1, 0,
			0.5f, 0.5f, 1, 1
		};

		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

		// now we define layout in vao
		vao = GL.GenVertexArray();

		GL.BindVertexArray(vao);

		GL.EnableVertexAttribArray(0);
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(index: 0, size: 2, type: VertexAttribPointerType.Float, normalized: false,
		                       stride: sizeof(float) * 4, // 1 row (2 floats for position, 2 uv floats)
		                       pointer: (IntPtr) 0);

		GL.VertexAttribPointer(index: 1, size: 2, type: VertexAttribPointerType.Float, normalized: false,
		                       stride: sizeof(float) * 4, // 1 row (2 floats for position, 2 uv floats)
		                       pointer: (IntPtr) 0);


		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}

	private static void CreateBoxRendererBuffers(ref int vao)
	{
		float[] vertices = new float[]
		                   {
			                   0.5f, 0.5f, 0.0f, 0, // top right
			                   0.5f, -0.5f, 0.0f, 0, // bottom right
			                   -0.5f, -0.5f, 0.0f, 0, // bottom left
			                   -0.5f, 0.5f, 0.0f, 0 // top left
		                   };
		int[] indices = new int[]
		                {
			                // note that we start from 0!
			                0, 1, 3, // first Triangle
			                1, 2, 3 // second Triangle
		                };
		int ebo;
		int vbo;
		// bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
		GL.GenVertexArrays(1, out vao);
		GL.BindVertexArray(vao);

		GL.GenBuffers(1, out vbo);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

		GL.GenBuffers(1, out ebo);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);

		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 4 * sizeof(float), (IntPtr) 0);
		GL.EnableVertexAttribArray(0);

		// note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

		// remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
		//glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
	}

	public static void CreateSpriteRendererBuffers(ref int vao)
	{
		int vbo = GL.GenBuffer();

		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

		float[] vertices =
		{
			-0.5f, -0.5f, 0, 0,
			0.5f, -0.5f, 1, 0,
			-0.5f, 0.5f, 0, 1,
			-0.5f, 0.5f, 0, 1,
			0.5f, -0.5f, 1, 0,
			0.5f, 0.5f, 1, 1
		};

		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

		// now we define layout in vao
		vao = GL.GenVertexArray();

		GL.BindVertexArray(vao);

		GL.EnableVertexAttribArray(0);
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(index: 0, size: 2, type: VertexAttribPointerType.Float, normalized: false,
		                       stride: sizeof(float) * 4, // 1 row (2 floats for position, 2 uv floats)
		                       pointer: (IntPtr) 0);

		GL.VertexAttribPointer(index: 1, size: 2, type: VertexAttribPointerType.Float, normalized: false,
		                       stride: sizeof(float) * 4, // 1 row (2 floats for position, 2 uv floats)
		                       pointer: (IntPtr) 0);


		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}

	const int triangles = 6 * 2; // Number of triangles rendered

	const int verticies_index = 0;
	const int colors_index = 1;

	public static void CreateModelBuffers(ref int vao)
	{
		GL.Enable(EnableCap.DepthTest);
		float[] vertices =
		{
			// Front face
			0.5f, 0.5f, 0.5f,
			-0.5f, 0.5f, 0.5f,
			-0.5f, -0.5f, 0.5f,
			0.5f, -0.5f, 0.5f,

			// Back face
			0.5f, 0.5f, -0.5f,
			-0.5f, 0.5f, -0.5f,
			-0.5f, -0.5f, -0.5f,
			0.5f, -0.5f, -0.5f,
		};

		float[] vertex_colors =
		{
			1.0f, 0.4f, 0.6f,
			1.0f, 0.9f, 0.2f,
			0.7f, 0.3f, 0.8f,
			0.5f, 0.3f, 1.0f,

			0.2f, 0.6f, 1.0f,
			0.6f, 1.0f, 0.4f,
			0.6f, 0.8f, 0.8f,
			0.4f, 0.8f, 0.8f,
		};
		uint[] triangle_indices =
		{
			// Front
			0, 1, 2,
			2, 3, 0,

			// Right
			0, 3, 7,
			7, 4, 0,

			// Bottom
			2, 6, 7,
			7, 3, 2,

			// Left
			1, 5, 6,
			6, 2, 1,

			// Back
			4, 7, 6,
			6, 5, 4,

			// Top
			5, 1, 0,
			0, 4, 5,
		};
		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		int triangles_ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangles_ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * triangle_indices.Length, triangle_indices, BufferUsageHint.StaticDraw);

		int  verticies_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, verticies_vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

		GL.VertexAttribPointer(verticies_index, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
		GL.EnableVertexAttribArray(verticies_index);


		int colors_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, colors_vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertex_colors.Length, vertex_colors, BufferUsageHint.StaticDraw);

		GL.VertexAttribPointer(colors_index, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
		GL.EnableVertexAttribArray(colors_index);

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
	/*private static void CreateSpriteRendererBuffers(ref int vao, ref int vbo)
	{
		vao = GL.GenVertexArray();
		vbo = GL.GenBuffer();

		BindVAO(vao);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

		List<float> verticesList = new List<float>();

		// 100 limit for now, but it can be dynamic too
		for (int i = 0; i < 1000; i++)
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

		bool batching = true;
		if (batching)
		{
			//
			//
			//
			//
			// create new vertex buffer for positions
			int vbo_positions = GL.GenBuffer();

			BindVAO(vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_positions);

			List<float> attribsList = new List<float>();

			// 100 limit for now, but it can be dynamic too\
			int x = 0;
			int y = 0;
			for (int i = 0; i < 1000; i++)
			{
				attribsList.AddRange(new float[]
				                     {
					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF,

					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF
				                     });

				x += 100;
				if (i % 40 == 0)
				{
					x = 0;
					y += 100;
				}
			}

			float[] attribs = attribsList.ToArray();

			GL.NamedBufferStorage(
			                      vbo_positions,
			                      sizeof(float) * attribs.Length,
			                      attribs,
			                      BufferStorageFlags.MapWriteBit);
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

			// ATTRIB: color -   1 int
			GL.VertexArrayAttribBinding(vao, 4, 1);
			GL.EnableVertexArrayAttrib(vao, 4);
			GL.VertexArrayAttribFormat(
			                           vao,
			                           4, // attribute index, from the shader location = 0
			                           1, // size of attribute, vec2
			                           VertexAttribType.UnsignedInt, // contains floats
			                           true,
			                           16); // relative offset, first item


			GL.VertexArrayVertexBuffer(vao, 1, vbo_positions, IntPtr.Zero, sizeof(float) * 4 + sizeof(byte));
		}
	}*/

	/*private static void CreateGradientRendererBuffers(ref int vao, ref int vbo)
	{
		vao = GL.GenVertexArray();
		vbo = GL.GenBuffer();

		BindVAO(vao);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

		float[] vertices = {-0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, 0.5f, 0.5f, -0.5f, 0.5f, 0.5f};

		GL.NamedBufferStorage(
		                      vbo,
		                      sizeof(float) * vertices.Length,
		                      vertices,
		                      BufferStorageFlags.MapWriteBit);

		
		GL.VertexArrayAttribBinding(vao, 0, 0);
		GL.EnableVertexArrayAttrib(vao, 0);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           0, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           false, // does not need to be normalized as it is already, floats ignore this flag anyway
		                           0); // relative offset, first item


		GL.VertexArrayAttribBinding(vao, 1, 0);
		GL.EnableVertexArrayAttrib(vao, 1);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           1, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           true, // does not need to be normalized as it is already, floats ignore this flag anyway
		                           8); // relative offset, first item

		GL.VertexArrayAttribBinding(1, 1, 1);
		GL.VertexArrayVertexBuffer(vao, 0, vbo, IntPtr.Zero, sizeof(float) * 4);
	}*/
}