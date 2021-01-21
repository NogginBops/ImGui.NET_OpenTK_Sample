using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace Dear_ImGui_Sample
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 25)]
        struct DrawElementsIndirectCommand
        {
            public uint Count;
            public uint InstanceCount;
            public uint FirstIndex;
            public uint BaseVertex;
            public uint BaseInstance;
        }

        class Buffer<T> where T : unmanaged
        {
            public readonly string Name;
            public int Handle;
            public int SizeInElements;

            public Buffer(string name, int handle, int sizeInElements)
            {
                Name = name;
                Handle = handle;
                SizeInElements = sizeInElements;
            }

            public static unsafe Buffer<T> Create(string name, int elements)
            {
                Util.CreateBuffer(name, out int handle);
                GL.NamedBufferStorage(handle, elements * sizeof(T), IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
                return new Buffer<T>(name, handle, elements);
            }
        }

        private bool _frameBegun;

        private Buffer<ImDrawVert> VertexBuffer2;
        private Buffer<ushort>     IndexBuffer2;

        private int VertexArray;
        private int VertexBuffer;
        private int VertexBufferSize;
        private int IndexBuffer;
        private int IndexBufferSize;

        private Buffer<long> TextureBuffer;
        //private int TextureBuffer;
        //private int TextureBufferSize;
        private RefList<long> TexturesList = new RefList<long>();

        private Buffer<int> DrawCallToTextureBuffer;
        //private int DrawCallToTextureBuffer;
        //private int DrawCallToTextureBufferSize;
        private RefList<int> CommandToTexture = new RefList<int>();

        // FXIME!!!!!
        private Buffer<Vector4> ScissorRectBuffer;

        private Buffer<DrawElementsIndirectCommand> DrawCommandBuffer;
        //private int DrawCommandBuffer;
        //private int DrawCommandBufferSize;
        private RefList<DrawElementsIndirectCommand> DrawCommands = new RefList<DrawElementsIndirectCommand>();

        private Texture FontTexture;
        private Shader _shader;
        
        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            _frameBegun = true;
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
            VertexBuffer2 = Buffer<ImDrawVert>.Create("ImGui Vertex Buffer", 10_000);
            IndexBuffer2 =  Buffer<ushort>.Create("ImGui Index Buffer", 2_000);

            VertexBufferSize = 10000;
            IndexBufferSize = 2000;
            Util.CreateVertexBuffer("ImGui", out VertexBuffer);
            Util.CreateElementBuffer("ImGui", out IndexBuffer);
            GL.NamedBufferData(VertexBuffer, VertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(IndexBuffer, IndexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            Util.CreateVertexArray("ImGui", out VertexArray);
            //GL.VertexArrayElementBuffer(VertexArray, IndexBuffer);
            GL.VertexArrayElementBuffer(VertexArray, IndexBuffer2.Handle);

            // FIXME: Should we start mapping our buffer like mapped ring buffers?
            TextureBuffer =           Buffer<long>.Create("ImGui Textures", 1);
            DrawCallToTextureBuffer = Buffer<int>.Create("ImGui Command to Texture", 10);

            DrawCommandBuffer = Buffer<DrawElementsIndirectCommand>.Create("ImGui Draw Commands", 10);

            RecreateFontDeviceTexture();
            
            string VertexSource = @"#version 460 core

struct Vertex 
{
    float position_x, position_y;
    float texCoord_u, texCoord_v;
    uint color;
};

// This is instead of VAOs and attributes
layout(binding = 0, std430) buffer VertexData
{
    Vertex vertices[];
};

out vec4 color;
out vec2 texCoord;

uniform mat4 projection_matrix;

void main()
{
    Vertex v = vertices[gl_VertexID];
    
    gl_Position = projection_matrix * vec4(v.position_x, v.position_y, 0, 1);
    texCoord = vec2(v.texCoord_u, v.texCoord_v);
    color = unpackUnorm4x8(v.color);
}";
            string FragmentSource = @"#version 460 core

#extension GL_ARB_bindless_texture : require

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

uniform sampler2D in_fontTexture;

uniform int drawcall;

layout(binding = 1, std430) buffer Textures
{
    layout(bindless_sampler) sampler2D textures[];
};

layout(binding = 2, std430) buffer DrawTextures
{
    int draw_id_texture[];
};

void main()
{
    int id = draw_id_texture[drawcall]; // gl_DrawID
    outputColor = color * texture(textures[id], texCoord);
}";
            _shader = new Shader("ImGui", VertexSource, FragmentSource);

            Util.CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            FontTexture = new Texture("ImGui Text Atlas", width, height, pixels);
            FontTexture.SetMagFilter(TextureMagFilter.Linear);
            FontTexture.SetMinFilter(TextureMinFilter.Linear);
            
            io.Fonts.SetTexID((IntPtr)FontTexture.CreateBindlessHandle());

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
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
        /// Updates ImGui input and IO configuration state.
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
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = wnd.MouseState;
            KeyboardState KeyboardState = wnd.KeyboardState;

            io.MouseDown[0] = MouseState[MouseButton.Left];
            io.MouseDown[1] = MouseState[MouseButton.Right];
            io.MouseDown[2] = MouseState[MouseButton.Middle];

            var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);
            
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
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

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            
            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        private static unsafe void UpdateBuffer<T>(Buffer<T> buffer, RefList<T> data) where T : unmanaged
        {
            // If we need to, create a new buffer
            if (buffer.SizeInElements < data.Count)
            {
                int newSize = (int)Math.Max(buffer.SizeInElements * 1.5f, data.Count);
                ReallocBuffer(buffer, newSize);
            }

            GL.NamedBufferSubData(buffer.Handle, IntPtr.Zero, data.Count * sizeof(T), ref data[0]);
        }

        private static unsafe void ReallocBuffer<T>(Buffer<T> buffer, int newSize) where T : unmanaged
        {
            GL.DeleteBuffer(buffer.Handle);
            Util.CreateBuffer(buffer.Name, out buffer.Handle);
            GL.NamedBufferStorage(buffer.Handle, newSize * sizeof(T), IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
            buffer.SizeInElements = newSize;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            TexturesList.Clear();
            CommandToTexture.Clear();
            int vertexCount = 0;
            int indexCount = 0;
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                vertexCount += cmd_list.VtxBuffer.Size;
                indexCount += cmd_list.IdxBuffer.Size;

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > VertexBufferSize)
                {
                    int newSize = (int)Math.Max(VertexBufferSize * 1.5f, vertexSize);
                    GL.NamedBufferData(VertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    VertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {VertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > IndexBufferSize)
                {
                    int newSize = (int)Math.Max(IndexBufferSize * 1.5f, indexSize);
                    GL.NamedBufferData(IndexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    IndexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {IndexBufferSize}");
                }

                // Build a list of unique textures and create a mapping from command to texture
                for (int commandIdx = 0; commandIdx < cmd_list.CmdBuffer.Size; commandIdx++)
                {
                    var command = cmd_list.CmdBuffer[commandIdx];
                    long textureHandle = command.TextureId.ToInt64();
                    if (TexturesList.TryGetIndexOf(textureHandle, out var index) == false)
                    {
                        TexturesList.Add(textureHandle);
                        index = TexturesList.Count - 1;

                        // Mark the texture as resident. We are going to need it.
                        GL.Arb.MakeTextureHandleResident(textureHandle);
                    }

                    CommandToTexture.Add(index);
                }
            }

            /**
            Console.WriteLine("Textures: ");
            for (int j = 0; j < TexturesList.Count; j++)
            {
                Console.Write(TexturesList[j]);
                if (j != TexturesList.Count - 1) Console.Write(", ");
            }
            Console.WriteLine();

            Console.WriteLine("Command to texture: ");
            for (int j = 0; j < CommandToTexture.Count; j++)
            {
                Console.Write(CommandToTexture[j]);
                if (j != CommandToTexture.Count - 1) Console.Write(", ");
            }
            Console.WriteLine();
            */

            if (VertexBuffer2.SizeInElements < vertexCount)
            {
                int newSize = (int)Math.Max(VertexBuffer2.SizeInElements * 1.5f, vertexCount);
                ReallocBuffer(VertexBuffer2, newSize);
                Console.WriteLine($"Resized vertex buffer! {newSize}");
            }

            if (IndexBuffer2.SizeInElements < indexCount)
            {
                int newSize = (int)Math.Max(IndexBuffer2.SizeInElements * 1.5f, indexCount);
                ReallocBuffer(IndexBuffer2, newSize);
                Console.WriteLine($"Resized index buffer! {newSize}");
            }

            UpdateBuffer(TextureBuffer, TexturesList);
            UpdateBuffer(DrawCallToTextureBuffer, CommandToTexture);

            DrawCommands.Clear();
            uint vtx_offset = 0;
            uint idx_offset = 0;
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                for (int j = 0; j < cmd_list.CmdBuffer.Size; j++)
                {
                    var cmd = cmd_list.CmdBuffer[j];

                    ref var command = ref DrawCommands.Add();

                    command.Count = cmd.ElemCount;
                    command.InstanceCount = 1;
                    command.FirstIndex = cmd.IdxOffset + idx_offset;
                    command.BaseVertex = cmd.VtxOffset + vtx_offset;
                    command.BaseInstance = 0;
                }

                // Upload this cmd list's vertices and indices, we have already resized them so we don't have to worry about running out of space.
                GL.NamedBufferSubData(VertexBuffer2.Handle, (IntPtr)vtx_offset, cmd_list.VtxBuffer.Size, cmd_list.VtxBuffer.Data);
                GL.NamedBufferSubData(IndexBuffer2.Handle, (IntPtr)idx_offset, cmd_list.IdxBuffer.Size, cmd_list.IdxBuffer.Data);

                vtx_offset += (uint)cmd_list.VtxBuffer.Size;
                idx_offset += (uint)cmd_list.IdxBuffer.Size;
            }

            UpdateBuffer(DrawCommandBuffer, DrawCommands);

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

            // This is instead of using a VAO with buffer attributes
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, VertexBuffer);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, TextureBuffer.Handle);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, DrawCallToTextureBuffer.Handle);

            // We only bind the VAO to be able to use the EBO
            GL.BindVertexArray(VertexArray);
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

                GL.NamedBufferSubData(VertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                Util.CheckGLError($"Data Vert {n}");

                GL.NamedBufferSubData(IndexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                Util.CheckGLError($"Data Idx {n}");

                int d = 0;
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        // FXIME!!!!! We want to put the scissor rectangles in a buffer and put that in the vertex thing.

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        Util.CheckGLError("Scissor");

                        GL.Uniform1(_shader.GetUniformLocation("drawcall"), d++);

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), (int)pcmd.VtxOffset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        Util.CheckGLError("Draw");
                    }
                }
            }

            // Mark all used textures not resident. We are done with them.
            for (int i = 0; i < TexturesList.Count; i++)
            {
                GL.Arb.MakeTextureHandleNonResident(TexturesList[i]);
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            FontTexture.Dispose();
            _shader.Dispose();
        }
    }
}
