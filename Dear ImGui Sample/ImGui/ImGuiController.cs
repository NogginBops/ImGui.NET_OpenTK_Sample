﻿/*using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Dear_ImGui_Sample;

/// <summary>
///         A modified version of Veldrid.ImGui's ImGuiRenderer.
///         Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
/// </summary>
public class ImGuiController : IDisposable
{
	private readonly List<char> PressedChars = new();

	private ImGuiTexture _fontTexture;
	private bool _frameBegun;
	private int _indexBuffer;
	private int _indexBufferSize;

	private Vector2 _scaleFactor = Vector2.One;
	private ImGuiShader _shader;

	private int _vertexArray;
	private int _vertexBuffer;
	private int _vertexBufferSize;
	private int _windowHeight;

	private int _windowWidth;

	/// <summary>
	///         Constructs a new ImGuiController.
	/// </summary>
	public ImGuiController(int width, int height)
	{
		_windowWidth = width;
		_windowHeight = height;

		IntPtr context = ImGui.CreateContext();
		ImGui.SetCurrentContext(context);
		ImGuiIOPtr io = ImGui.GetIO();

		io.Fonts.AddFontDefault();

		io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

		io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

		CreateDeviceResources();
		SetKeyMappings();

		SetPerFrameImGuiData(1f / 60f);

		ImGui.NewFrame();
		_frameBegun = true;
	}

	/// <summary>
	///         Frees all graphics resources used by the renderer.
	/// </summary>
	public void Dispose()
	{
		_fontTexture.Dispose();
		_shader.Dispose();
	}

	public void WindowResized(int width, int height)
	{
		_windowWidth = width;
		_windowHeight = height;
	}

	public void DestroyDeviceObjects()
	{
		Dispose();
	}

	public void CreateDeviceResources()
	{
		Util.CreateVertexArray("ImGui", out _vertexArray);

		_vertexBufferSize = 10000;
		_indexBufferSize = 2000;

		Util.CreateVertexBuffer("ImGui", out _vertexBuffer);
		Util.CreateElementBuffer("ImGui", out _indexBuffer);
		GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
		GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

		RecreateFontDeviceTexture();

		string VertexSource = @"#version 460 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
		string FragmentSource = @"#version 460 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";
		_shader = new ImGuiShader("ImGui", VertexSource, FragmentSource);

		GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
		GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

		GL.EnableVertexArrayAttrib(_vertexArray, 0);
		GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
		GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

		GL.EnableVertexArrayAttrib(_vertexArray, 1);
		GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
		GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

		GL.EnableVertexArrayAttrib(_vertexArray, 2);
		GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
		GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);

		Util.CheckGLError("End of ImGui setup");
	}

	/// <summary>
	///         Recreates the device texture used to render text.
	/// </summary>
	public void RecreateFontDeviceTexture()
	{
		ImGuiIOPtr io = ImGui.GetIO();
		io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

		_fontTexture = new ImGuiTexture("ImGui Text Atlas", width, height, pixels);
		_fontTexture.SetMagFilter(TextureMagFilter.Linear);
		_fontTexture.SetMinFilter(TextureMinFilter.Linear);

		io.Fonts.SetTexID((IntPtr) _fontTexture.GLTexture);

		io.Fonts.ClearTexData();
	}

	/// <summary>
	///         Renders the ImGui draw list data.
	///         This method requires a <see cref="GraphicsDevice" /> because it may create new DeviceBuffers if the size of
	///         vertex
	///         or index data has increased beyond the capacity of the existing buffers.
	///         A <see cref="CommandList" /> is needed to submit drawing and resource update commands.
	/// </summary>
	public void Render()
	{
		if (_frameBegun)
		{
			_frameBegun = false;
			ImGui.Render();
			RenderImDrawData(ImGui.GetDrawData());
		}
	}

	/// <summary>
	///         Updates ImGui input and IO configuration state.
	/// </summary>
	public void Update(GameWindow wnd, float deltaSeconds)
	{
		if (_frameBegun)
		{
			ImGui.Render();
		}

		SetPerFrameImGuiData(deltaSeconds);
		UpdateImGuiInput(wnd);

		_frameBegun = true;
		ImGui.NewFrame();
	}

	/// <summary>
	///         Sets per-frame data based on the associated window.
	///         This is called by Update(float).
	/// </summary>
	private void SetPerFrameImGuiData(float deltaSeconds)
	{
		ImGuiIOPtr io = ImGui.GetIO();
		io.DisplaySize = new Vector2(
		                             _windowWidth / _scaleFactor.X,
		                             _windowHeight / _scaleFactor.Y);
		io.DisplayFramebufferScale = _scaleFactor;
		io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
	}

	private void UpdateImGuiInput(GameWindow wnd)
	{
		ImGuiIOPtr io = ImGui.GetIO();

		MouseState MouseState = wnd.MouseState;
		KeyboardState KeyboardState = wnd.KeyboardState;

		io.MouseDown[0] = MouseState[MouseButton.Left];
		io.MouseDown[1] = MouseState[MouseButton.Right];
		io.MouseDown[2] = MouseState[MouseButton.Middle];

		Vector2i screenPoint = new((int) MouseState.X, (int) MouseState.Y);
		Vector2i point = screenPoint; //wnd.PointToClient(screenPoint);
		io.MousePos = new Vector2(point.X, point.Y);

		foreach (Keys key in Enum.GetValues(typeof(Keys)))
		{
			if (key == Keys.Unknown)
			{
				continue;
			}

			io.KeysDown[(int) key] = KeyboardState.IsKeyDown(key);
		}

		foreach (char c in PressedChars) io.AddInputCharacter(c);
		PressedChars.Clear();

		io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
		io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
		io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
		io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
	}

	internal void PressChar(char keyChar)
	{
		PressedChars.Add(keyChar);
	}

	internal void MouseScroll(Engine.Vector2 offset)
	{
		ImGuiIOPtr io = ImGui.GetIO();

		io.MouseWheel = offset.Y;
		io.MouseWheelH = offset.X;
	}

	private static void SetKeyMappings()
	{
		ImGuiIOPtr io = ImGui.GetIO();
		io.KeyMap[(int) ImGuiKey.Tab] = (int) Keys.Tab;
		io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) Keys.Left;
		io.KeyMap[(int) ImGuiKey.RightArrow] = (int) Keys.Right;
		io.KeyMap[(int) ImGuiKey.UpArrow] = (int) Keys.Up;
		io.KeyMap[(int) ImGuiKey.DownArrow] = (int) Keys.Down;
		io.KeyMap[(int) ImGuiKey.PageUp] = (int) Keys.PageUp;
		io.KeyMap[(int) ImGuiKey.PageDown] = (int) Keys.PageDown;
		io.KeyMap[(int) ImGuiKey.Home] = (int) Keys.Home;
		io.KeyMap[(int) ImGuiKey.End] = (int) Keys.End;
		io.KeyMap[(int) ImGuiKey.Delete] = (int) Keys.Delete;
		io.KeyMap[(int) ImGuiKey.Backspace] = (int) Keys.Backspace;
		io.KeyMap[(int) ImGuiKey.Enter] = (int) Keys.Enter;
		io.KeyMap[(int) ImGuiKey.Escape] = (int) Keys.Escape;
		io.KeyMap[(int) ImGuiKey.A] = (int) Keys.A;
		io.KeyMap[(int) ImGuiKey.C] = (int) Keys.C;
		io.KeyMap[(int) ImGuiKey.V] = (int) Keys.V;
		io.KeyMap[(int) ImGuiKey.X] = (int) Keys.X;
		io.KeyMap[(int) ImGuiKey.Y] = (int) Keys.Y;
		io.KeyMap[(int) ImGuiKey.Z] = (int) Keys.Z;
	}

	private void RenderImDrawData(ImDrawDataPtr draw_data)
	{
		if (draw_data.CmdListsCount == 0)
		{
			return;
		}

		for (int i = 0; i < draw_data.CmdListsCount; i++)
		{
			ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

			int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
			if (vertexSize > _vertexBufferSize)
			{
				int newSize = (int) Math.Max(_vertexBufferSize * 1.5f, vertexSize);
				GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
				_vertexBufferSize = newSize;

				Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
			}

			int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
			if (indexSize > _indexBufferSize)
			{
				int newSize = (int) Math.Max(_indexBufferSize * 1.5f, indexSize);
				GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
				_indexBufferSize = newSize;

				Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
			}
		}

		// Setup orthographic projection matrix into our constant buffer
		ImGuiIOPtr io = ImGui.GetIO();
		Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
		                                                  0.0f,
		                                                  io.DisplaySize.X,
		                                                  io.DisplaySize.Y,
		                                                  0.0f,
		                                                  -1.0f,
		                                                  1.0f);

		_shader.UseShader();
		GL.UniformMatrix4(_shader.GetUniformLocation("projection_matrix"), false, ref mvp);
		GL.Uniform1(_shader.GetUniformLocation("in_fontTexture"), 0);
		Util.CheckGLError("Projection");

		BufferCache.BindVAO(_vertexArray);
		Util.CheckGLError("VAO");

		draw_data.ScaleClipRects(io.DisplayFramebufferScale);

		GL.Enable(EnableCap.Blend);
		GL.Enable(EnableCap.ScissorTest);
		GL.BlendEquation(BlendEquationMode.FuncAdd);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.Disable(EnableCap.CullFace);
		GL.Disable(EnableCap.DepthTest);

		// Render command lists
		for (int n = 0; n < draw_data.CmdListsCount; n++)
		{
			ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

			GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
			Util.CheckGLError($"Data Vert {n}");

			GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
			Util.CheckGLError($"Data Idx {n}");

			for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
			{
				ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
				if (pcmd.UserCallback != IntPtr.Zero)
				{
					throw new NotImplementedException();
				}

				GL.ActiveTexture(TextureUnit.Texture0);
				TextureCache.BindTexture((int) pcmd.TextureId);
				Util.CheckGLError("Texture");

				// We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
				Vector4 clip = pcmd.ClipRect;
				GL.Scissor((int) clip.X, _windowHeight - (int) clip.W, (int) (clip.Z - clip.X), (int) (clip.W - clip.Y));
				Util.CheckGLError("Scissor");

				if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
				{
					GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int) pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr) (pcmd.IdxOffset * sizeof(ushort)), (int) pcmd.VtxOffset);
				}
				else
				{
					GL.DrawElements(BeginMode.Triangles, (int) pcmd.ElemCount, DrawElementsType.UnsignedShort, (int) pcmd.IdxOffset * sizeof(ushort));
				}

				Util.CheckGLError("Draw");
			}
		}

		GL.Disable(EnableCap.Blend);
		GL.Disable(EnableCap.ScissorTest);
	}
}*/