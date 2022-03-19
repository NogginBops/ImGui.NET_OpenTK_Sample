using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using System.Runtime.InteropServices;

namespace Dear_ImGui_Sample
{
    public class ImGuiController : IDisposable
    {
        // FIXME: Maybe we can share these using context sharing? We would need to create a new VAO though.
        /*struct GLDrawData
        {
            private int _vertexArray;
            private int _vertexBuffer;
            private int _vertexBufferSize;
            private int _indexBuffer;
            private int _indexBufferSize;

            private int _fontTexture;

            private int _shader;
            private int _shaderFontTextureLocation;
            private int _shaderProjectionMatrixLocation;
        }*/

        private bool _frameBegun;

        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        private int _fontTexture;

        private int _shader;
        private int _shaderFontTextureLocation;
        private int _shaderProjectionMatrixLocation;
        
        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        private static bool KHRDebugAvailable = false;

        private GCHandle _mainWindowHandle;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            KHRDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);
        }

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void ImGuiPlatformIO_Set_Platform_GetWindowPos(ImGuiPlatformIO* platform_io, IntPtr funcPtr);
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void ImGuiPlatformIO_Set_Platform_GetWindowSize(ImGuiPlatformIO* platform_io, IntPtr funcPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void Platform_CreateWindow(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void Platform_DestroyWindow(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Platform_ShowWindow(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Platform_SetWindowPos(ImGuiViewportPtr vp, Vector2 pos);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void Platform_GetWindowPos(ImGuiViewportPtr vp, out Vector2 pos);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Platform_SetWindowSize(ImGuiViewportPtr vp, Vector2 size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void Platform_GetWindowSize(ImGuiViewportPtr vp, out Vector2 size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Platform_SetWindowFocus(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte Platform_GetWindowFocus(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte Platform_GetWindowMinimized(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Platform_SetWindowTitle(ImGuiViewportPtr vp, IntPtr title);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Platform_UpdateWindow(ImGuiViewportPtr vp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Renderer_RenderWindow(ImGuiViewportPtr vp, IntPtr render_arg);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Renderer_SwapBuffers(ImGuiViewportPtr vp, IntPtr render_arg);

        private Platform_CreateWindow CreateWindowDelegate;
        private Platform_DestroyWindow DestroyWindowDelegate;
        private Platform_ShowWindow ShowWindowDelegate;
        private Platform_SetWindowPos SetWindowPosDelegate;
        private Platform_GetWindowPos GetWindowPosDelegate;
        private Platform_SetWindowSize SetWindowSizeDelegate;
        private Platform_GetWindowSize GetWindowSizeDelegate;
        private Platform_SetWindowFocus SetWindowFocusDelegate;
        private Platform_GetWindowFocus GetWindowFocusDelegate;
        private Platform_GetWindowMinimized GetWindowMinimizedDelegate;
        private Platform_SetWindowTitle SetWindowTitleDelegate;

        private Platform_UpdateWindow UpdateWindowDelegate;

        private Renderer_RenderWindow RenderWindowDelegate;
        private Renderer_SwapBuffers SwapBuffersDelegate;

        private unsafe void CreateWindow(ImGuiViewportPtr vp)
        {
            // FIXME: Transparent window!

            NativeWindowSettings settings = new NativeWindowSettings()
            {
                StartVisible = false,
                StartFocused = false,
                WindowState = OpenTK.Windowing.Common.WindowState.Normal,
                Location = new Vector2i((int)vp.Pos.X, (int)vp.Pos.Y),
                Size = new Vector2i((int)vp.Size.X, (int)vp.Size.Y),

                // FIXME: Can this actually be used? We still need to setup the VAO in the new context.
                SharedContext = ((GameWindow)_mainWindowHandle.Target).Context,
            };

            if (vp.Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
            {
                // FIXME: Figure out why this causes the window to now show up.
                //settings.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden;
                //settings.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden;
            }
            else
            {
                settings.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Resizable;
            }

            // Create the window and allocate the handle.
            GameWindow gw = new GameWindow(GameWindowSettings.Default, settings);

            if (vp.Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
            {
                // FIXME: Figure out why we need to do this after the window is created for it work.
                gw.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden;
            }

            var handle = GCHandle.Alloc(gw);
            vp.PlatformUserData = (IntPtr)handle;

            gw.Resize += (e) => { vp.PlatformRequestResize = true; };
            gw.Move += (e) => { vp.PlatformRequestMove = true; };
            gw.Closing += (e) => { vp.PlatformRequestClose = true; };
        }

        private static unsafe void DestroyWindow(ImGuiViewportPtr vp)
        {
            Console.WriteLine("DestroyWindow");
            if (vp.PlatformUserData != IntPtr.Zero)
            {
                var handle = GCHandle.FromIntPtr(vp.PlatformUserData);
                GameWindow window = (GameWindow)handle.Target;
                window.Dispose();

                handle.Free();
                vp.PlatformUserData = IntPtr.Zero;
            }
        }

        private static unsafe void ShowWindow(ImGuiViewportPtr vp)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            Console.WriteLine("ShowWindow: " + window.Title);
            window.IsVisible = true;
        }
        private static unsafe void SetWindowPos(ImGuiViewportPtr vp, Vector2 pos)
        {
            Console.WriteLine("SetWindowPos: " + pos);
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            window.Location = (Vector2i)pos;
        }
        public static unsafe void GetWindowPos(ImGuiViewportPtr vp, out Vector2 pos)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            pos = window.Location.ToVector2();
        }
        public static unsafe void SetWindowSize(ImGuiViewportPtr vp, Vector2 size)
        {
            Console.WriteLine("SetWindowSize: " + size);
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            window.Size = (Vector2i)size;
        }
        public static unsafe void GetWindowSize(ImGuiViewportPtr vp, out Vector2 size)
        {
            Console.WriteLine("GetWindowSize");
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            size = window.Size;
        }
        public static unsafe void SetWindowFocus(ImGuiViewportPtr vp)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            window.Focus();
        }
        public static unsafe byte GetWindowFocus(ImGuiViewportPtr vp)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            return (byte)(window.IsFocused ? 1 : 0);
        }
        public static unsafe byte GetWindowMinimized(ImGuiViewportPtr vp)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            bool minimized = window.WindowState == OpenTK.Windowing.Common.WindowState.Minimized;
            return (byte) (minimized ? 1 : 0);
        }
        public static unsafe void SetWindowTitle(ImGuiViewportPtr vp, IntPtr titlePtr)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            string title = Marshal.PtrToStringUTF8(titlePtr);
            window.Title = title;
            Console.WriteLine("SetWindowTitle: " + title);
        }

        public static unsafe void UpdateWindow(ImGuiViewportPtr vp)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;

            window.ProcessEvents();
        }

        public unsafe void RenderWindow(ImGuiViewportPtr vp, IntPtr render_arg)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            window.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // FIXME: We need to have all of the buffers created for this context to be able to draw stuff!
            RenderImDrawData(vp.DrawData);
        }

        public static unsafe void SwapBuffers(ImGuiViewportPtr vp, IntPtr render_arg)
        {
            GameWindow window = (GameWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target;
            window.SwapBuffers();
        }

        /// <summary>
        /// Constructs a new ImGuiController with multi-viewport support.
        /// </summary>
        /// <param name="window"></param>
        public unsafe ImGuiController(GameWindow window) : this(window.ClientSize.X, window.ClientSize.Y)
        {
            var platformIO = ImGui.GetPlatformIO();

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
            io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

            _mainWindowHandle = GCHandle.Alloc(window);
            platformIO.Viewports[0].PlatformUserData = (IntPtr)_mainWindowHandle;

            // Setup multiple viewports

            CreateWindowDelegate = CreateWindow;
            DestroyWindowDelegate = DestroyWindow;
            ShowWindowDelegate = ShowWindow;
            SetWindowPosDelegate = SetWindowPos;
            GetWindowPosDelegate = GetWindowPos;
            SetWindowSizeDelegate = SetWindowSize;
            GetWindowSizeDelegate = GetWindowSize;
            SetWindowFocusDelegate = SetWindowFocus;
            GetWindowFocusDelegate = GetWindowFocus;
            GetWindowMinimizedDelegate = GetWindowMinimized;
            SetWindowTitleDelegate = SetWindowTitle;

            UpdateWindowDelegate = UpdateWindow;

            RenderWindowDelegate = RenderWindow;
            SwapBuffersDelegate = SwapBuffers;

            platformIO.Platform_CreateWindow = Marshal.GetFunctionPointerForDelegate(CreateWindowDelegate);
            platformIO.Platform_DestroyWindow = Marshal.GetFunctionPointerForDelegate(DestroyWindowDelegate);
            platformIO.Platform_ShowWindow = Marshal.GetFunctionPointerForDelegate(ShowWindowDelegate);
            platformIO.Platform_SetWindowPos = Marshal.GetFunctionPointerForDelegate(SetWindowPosDelegate);
            //platformIO.Platform_GetWindowPos = Marshal.GetFunctionPointerForDelegate(GetWindowPosDelegate);
            platformIO.Platform_SetWindowSize = Marshal.GetFunctionPointerForDelegate(SetWindowSizeDelegate);
            //platformIO.Platform_GetWindowSize = Marshal.GetFunctionPointerForDelegate(GetWindowSizeDelegate);
            platformIO.Platform_SetWindowFocus = Marshal.GetFunctionPointerForDelegate(SetWindowFocusDelegate);
            platformIO.Platform_GetWindowFocus = Marshal.GetFunctionPointerForDelegate(GetWindowFocusDelegate);
            platformIO.Platform_GetWindowMinimized= Marshal.GetFunctionPointerForDelegate(GetWindowMinimizedDelegate);
            platformIO.Platform_SetWindowTitle = Marshal.GetFunctionPointerForDelegate(SetWindowTitleDelegate);

            platformIO.Platform_UpdateWindow = Marshal.GetFunctionPointerForDelegate(UpdateWindowDelegate);

            platformIO.Renderer_RenderWindow = Marshal.GetFunctionPointerForDelegate(RenderWindowDelegate);
            platformIO.Renderer_SwapBuffers = Marshal.GetFunctionPointerForDelegate(SwapBuffersDelegate);

            ImGuiPlatformIO_Set_Platform_GetWindowPos(platformIO, Marshal.GetFunctionPointerForDelegate(GetWindowPosDelegate));
            ImGuiPlatformIO_Set_Platform_GetWindowSize(platformIO, Marshal.GetFunctionPointerForDelegate(GetWindowSizeDelegate));

            var monitors = Monitors.GetMonitors();

            Marshal.FreeHGlobal(platformIO.NativePtr->Monitors.Data);
            IntPtr data = Marshal.AllocHGlobal(Unsafe.SizeOf<ImGuiPlatformMonitor>() * monitors.Count);
            var vec = new ImVector<ImGuiPlatformMonitor>(monitors.Count, monitors.Count, data);
            platformIO.NativePtr->Monitors = Unsafe.As<ImVector<ImGuiPlatformMonitor>, ImVector>(ref vec);
            for (int i = 0; i < monitors.Count; i++)
            {
                // FIXME: Get proper dpi scaling!
                vec[i].DpiScale = 1f;
                var area = monitors[i].ClientArea;
                var pos = area.Min.ToVector2();
                var size = area.Size.ToVector2();

                var workArea = monitors[i].WorkArea;
                var workPos = workArea.Min.ToVector2();
                var workSize = workArea.Size.ToVector2();

                vec[i].MainPos = Unsafe.As<Vector2, System.Numerics.Vector2>(ref pos);
                vec[i].MainSize = Unsafe.As<Vector2, System.Numerics.Vector2>(ref size);

                // FIXME!
                //vec[i].WorkPos = Unsafe.As<Vector2, System.Numerics.Vector2>(ref workPos);
                //vec[i].WorkSize= Unsafe.As<Vector2, System.Numerics.Vector2>(ref workSize);

                vec[i].WorkPos = vec[i].MainPos;
                vec[i].WorkSize = vec[i].MainSize;
            }
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
            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);
            LabelObject(ObjectLabelIdentifier.VertexArray, _vertexArray, "ImGui");

            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core

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
            string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

            _shader = CreateProgram("ImGui", VertexSource, FragmentSource);
            _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
            _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            _fontTexture = GL.GenTexture();
            GL.ActiveTexture(0);
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
            LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
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

            var viewports = ImGui.GetPlatformIO().Viewports;
            for (int i = 1; i < viewports.Size; i++)
            {
                var v = viewports[i];
                GameWindow window = (GameWindow)GCHandle.FromIntPtr(v.PlatformUserData).Target;
                window.ProcessEvents();
            }
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

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(_vertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    
                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
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

            GL.UseProgram(_shader);
            GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
            GL.Uniform1(_shaderFontTextureLocation, 0);
            CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            CheckGLError("VAO");

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

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                CheckGLError($"Data Vert {n}");

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                CheckGLError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            int vertexOffset;
                            unchecked
                            {
                                vertexOffset = (int)pcmd.VtxOffset;
                            }
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), vertexOffset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        CheckGLError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vertexArray);
            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);

            GL.DeleteTexture(_fontTexture);
            GL.DeleteProgram(_shader);

            if (_mainWindowHandle.IsAllocated)
            {
                _mainWindowHandle.Free();
            }
        }

        public static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
        {
            if (KHRDebugAvailable)
                GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
        }

        static bool IsExtensionSupported(string name)
        {
            int n = GL.GetInteger(GetPName.NumExtensions);
            for (int i = 0; i < n; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension == name) return true;
            }

            return false;
        }

        public static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
        {
            int program = GL.CreateProgram();
            LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

            int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
            }

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        private static int CompileShader(string name, ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
            }

            return shader;
        }

        public static void CheckGLError(string title)
        {
            ErrorCode error;
            int i = 1;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                Debug.Print($"{title} ({i++}): {error}");
            }
        }
    }
}
