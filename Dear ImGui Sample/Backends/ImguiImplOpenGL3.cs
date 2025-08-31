using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Dear_ImGui_Sample.Backends
{
    public unsafe static class ImguiImplOpenGL3
    {
        // See: https://github.com/ImGuiNET/ImGui.NET/issues/527
        internal struct ImDrawCmd_fixed
        {
            public Vector4 ClipRect;
            public ulong TextureId;
            public uint VtxOffset;
            public uint IdxOffset;
            public uint ElemCount;
            public nint UserCallback;
            public unsafe void* UserCallbackData;
            public int UserCallbackDataSize;
            public int UserCallbackDataOffset;
        }
        internal struct ImDrawCmdPtr_fixed
        {
            public unsafe ImDrawCmd_fixed* NativePtr { get; }

            public unsafe nint GetTexID()
            {
                return ImGuiNative.ImDrawCmd_GetTexID((ImDrawCmd*)NativePtr);
            }
        }

        struct RendererData {
            public int FontTexture;
            public int ShaderHandle;
            public int UniformLocationTex;
            public int UniformLocationProjMtx;
            public int AttribLocationVtxPos;
            public int AttribLocationVtxUV;
            public int AttribLocationVtxColor;
            public int VboHandle;
            public int EboHandle;
            // FIXME: ??
            public bool HasPolygonMode;
            public bool HasClipOrigin;

            public int GlslVersion;
        }

        static RendererData* GetBackendData()
        {
            return ImGui.GetCurrentContext() == 0 ? null : (RendererData*)ImGui.GetIO().BackendRendererUserData;
        }

        public static bool Init()
        {
            var io = ImGui.GetIO();

            RendererData* bd = (RendererData*)NativeMemory.AllocZeroed((uint)sizeof(RendererData));
            bd->GlslVersion = 410;

            io.BackendRendererUserData = (IntPtr)bd;
            io.NativePtr->BackendRendererName = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("opentk_impl_opengl3"u8));

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

            InitMultiViewportSupport();

            return true;
        }

        public static void Shutdown()
        {
            var io = ImGui.GetIO();

            RendererData* bd = (RendererData*)io.NativePtr->BackendRendererUserData;

            ShutdownMultiViewportSupport();

            io.NativePtr->BackendRendererName = null;
            io.NativePtr->BackendRendererUserData = null;

            io.BackendFlags &= ~(ImGuiBackendFlags.RendererHasVtxOffset);

            NativeMemory.Free(bd);
        }

        public static void NewFrame()
        {
            RendererData* bd = GetBackendData();

            if (bd->ShaderHandle == 0)
            {
                CreateDeviceObjects();
            }
            if (bd->FontTexture == 0)
            {
                CreateFontsTexture();
            }
        }

        public static void SetupRenderState(ImDrawDataPtr drawData, int fbWidth, int fbHeight, int vao)
        {
            RendererData* bd = GetBackendData();

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.ScissorTest);
            // FIXME: check for 3.1
            GL.Disable(EnableCap.PrimitiveRestart);

            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

            bool clip_origin_lower_left = true;
            ClipOrigin clip_origin = (ClipOrigin)GL.GetInteger(GetPName.ClipOrigin);
            if (clip_origin == ClipOrigin.UpperLeft)
                clip_origin_lower_left = false;

            GL.Viewport(0, 0, fbWidth, fbHeight);
            float L = drawData.DisplayPos.X;
            float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
            float T = drawData.DisplayPos.Y;
            float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
            if (clip_origin_lower_left == false)
                (T, B) = (B, T); // Swap top and bottom if origin is upper left.
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(L, R, B, T, -1, 1);
            GL.UseProgram(bd->ShaderHandle);
            GL.Uniform1(bd->UniformLocationTex, 0);
            GL.UniformMatrix4(bd->UniformLocationProjMtx, true, ref mvp);

            GL.BindSampler(0, 0);

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bd->VboHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bd->EboHandle);
            GL.EnableVertexAttribArray(bd->AttribLocationVtxPos);
            GL.EnableVertexAttribArray(bd->AttribLocationVtxUV);
            GL.EnableVertexAttribArray(bd->AttribLocationVtxColor);
            GL.VertexAttribPointer(bd->AttribLocationVtxPos, 2, VertexAttribPointerType.Float, false, sizeof(ImDrawVert),  0);
            GL.VertexAttribPointer(bd->AttribLocationVtxUV, 2, VertexAttribPointerType.Float, false, sizeof(ImDrawVert),  8);
            GL.VertexAttribPointer(bd->AttribLocationVtxColor, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(ImDrawVert), 16);
        }

        public static void RenderDrawData(ImDrawDataPtr drawData)
        {
            int fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
            int fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
            if (fbWidth <= 0 || fbHeight <= 0)
                return;

            int last_active_texture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int last_program = GL.GetInteger(GetPName.CurrentProgram);
            int last_texture = GL.GetInteger(GetPName.TextureBinding2D);
            int last_sampler = GL.GetInteger(GetPName.SamplerBinding);
            int last_array_buffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int last_vao = GL.GetInteger(GetPName.VertexArrayBinding);
            // OpenGL 3.0 & 3.1 have separate polygon modes for front and back.
            Span<int> last_polygon_mode = stackalloc int[2];
            GL.GetInteger(GetPName.PolygonMode, out last_polygon_mode[0]);
            Span<int> last_viewport = stackalloc int[4];
            GL.GetInteger(GetPName.Viewport, out last_viewport[0]);
            Span<int> last_scissor_box = stackalloc int[4];
            GL.GetInteger(GetPName.ScissorBox, out last_scissor_box[0]);
            int last_blend_src_rgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int last_blend_dst_rgb = GL.GetInteger(GetPName.BlendDstRgb);
            int last_blend_src_alpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int last_blend_dst_alpha = GL.GetInteger(GetPName.BlendDstAlpha);
            int last_blend_equation_rgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int last_blend_equation_alpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            bool last_enable_blend = GL.IsEnabled(EnableCap.Blend);
            bool last_enable_cull_face = GL.IsEnabled(EnableCap.CullFace);
            bool last_enable_depth_test = GL.IsEnabled(EnableCap.DepthTest);
            bool last_enable_stencil_test = GL.IsEnabled(EnableCap.StencilTest);
            bool last_enable_scissor_test = GL.IsEnabled(EnableCap.ScissorTest);
            // FIXME: Check for >= 3.1
            bool last_enable_primitive_restart = GL.IsEnabled(EnableCap.PrimitiveRestart);

            int vao = GL.GenVertexArray();
            SetupRenderState(drawData, fbWidth, fbHeight, vao);

            var clipOff = drawData.DisplayPos;
            var clipScale = drawData.FramebufferScale;
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr drawList = drawData.CmdLists[n];

                nint vtx_buffer_size = drawList.VtxBuffer.Size * (int)sizeof(ImDrawVert);
                nint idx_buffer_size = drawList.IdxBuffer.Size * (int)sizeof(ushort);
                GL.BufferData(BufferTarget.ArrayBuffer, vtx_buffer_size, drawList.VtxBuffer.Data, BufferUsageHint.StreamDraw);
                GL.BufferData(BufferTarget.ElementArrayBuffer, idx_buffer_size, drawList.IdxBuffer.Data, BufferUsageHint.StreamDraw);

                for (int cmd_i = 0; cmd_i < drawList.CmdBuffer.Size; cmd_i++)
                {
                    // FIXME: This is a hack to make 32-bit builds work. See: https://github.com/ImGuiNET/ImGui.NET/issues/527
                    ImDrawCmdPtr_fixed cmdPtr = new ImPtrVector<ImDrawCmdPtr_fixed>(drawList.NativePtr->CmdBuffer, sizeof(ImDrawCmd_fixed))[cmd_i];
                    ref ImDrawCmd_fixed cmd = ref Unsafe.AsRef<ImDrawCmd_fixed>(cmdPtr.NativePtr);
                    if (cmd.UserCallback != 0)
                    {
                        // FIXME: ...
                        nint ImDrawCallback_ResetRenderState = -8;
                        if (cmd.UserCallback == ImDrawCallback_ResetRenderState)
                        {
                            SetupRenderState(drawData, fbWidth, fbHeight, vao);
                        }
                        else
                        {
                            throw new NotImplementedException("User callbacks are not implemented yet...");
                        }
                    }
                    else
                    {
                        Vector2 clip_min = new((cmd.ClipRect.X -clipOff.X) *clipScale.X, (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y);
                        Vector2 clip_max = new((cmd.ClipRect.Z -clipOff.X) *clipScale.X, (cmd.ClipRect.W - clipOff.Y) * clipScale.Y);
                        if (clip_max.X <= clip_min.X || clip_max.Y <= clip_min.Y)
                            continue;

                        GL.Scissor((int)clip_min.X, (int)((float)fbHeight - clip_max.Y), (int)(clip_max.X - clip_min.X), (int)(clip_max.Y - clip_min.Y));

                        GL.BindTexture(TextureTarget.Texture2D, (int)cmdPtr.GetTexID());
                        
                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)cmd.ElemCount, DrawElementsType.UnsignedShort, (int)(cmd.IdxOffset * sizeof(ushort)), (int)cmd.VtxOffset);
                    }
                }
            }

            GL.DeleteVertexArray(vao);

            if (last_program == 0 || GL.IsProgram(last_program)) GL.UseProgram(last_program);
            GL.BindTexture(TextureTarget.Texture2D, last_texture);
            GL.BindSampler(0, last_sampler);
            GL.ActiveTexture((TextureUnit)last_active_texture);
            GL.BindVertexArray(last_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, last_array_buffer);
            GL.BlendEquationSeparate((BlendEquationMode)last_blend_equation_rgb, (BlendEquationMode)last_blend_equation_alpha);
            GL.BlendFuncSeparate((BlendingFactorSrc)last_blend_src_rgb, (BlendingFactorDest)last_blend_dst_rgb, (BlendingFactorSrc)last_blend_src_alpha, (BlendingFactorDest)last_blend_dst_alpha);
            if (last_enable_blend) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (last_enable_cull_face) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (last_enable_depth_test) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (last_enable_stencil_test) GL.Enable(EnableCap.StencilTest); else GL.Disable(EnableCap.StencilTest);
            if (last_enable_scissor_test) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
            if (last_enable_primitive_restart) GL.Enable(EnableCap.PrimitiveRestart); else GL.Disable(EnableCap.PrimitiveRestart);

            if (true)
            {
                // FIXME:
                // if (bd->HasPolygonMode) {
                //     if (bd->GlVersion <= 310 || bd->GlProfileIsCompat) {
                //          glPolygonMode(GL_FRONT, (GLenum)last_polygon_mode[0]);
                //          glPolygonMode(GL_BACK, (GLenum)last_polygon_mode[1]);
                //     } else {
                //          glPolygonMode(GL_FRONT_AND_BACK, (GLenum)last_polygon_mode[0]);
                //     }
                // }
                GL.PolygonMode(TriangleFace.FrontAndBack, (PolygonMode)last_polygon_mode[0]);
            }

            GL.Viewport(last_viewport[0], last_viewport[1], last_viewport[2], last_viewport[3]);
            GL.Scissor(last_scissor_box[0], last_scissor_box[1], last_scissor_box[2], last_scissor_box[3]);
        }

        static void CreateFontsTexture()
        {
            var io = ImGui.GetIO();
            RendererData* bd = GetBackendData();

            ImGuiNative.ImFontAtlas_AddFontDefault(io.Fonts.NativePtr, null);
            //io.Fonts.AddFontDefault();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

            int last_texture = GL.GetInteger(GetPName.TextureBinding2D);
            bd->FontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, bd->FontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)pixels);

            io.Fonts.SetTexID(bd->FontTexture);

            GL.BindTexture(TextureTarget.Texture2D, last_texture);
        }

        static void DestroyFontsTexture()
        {
            var io = ImGui.GetIO();
            RendererData* bd = GetBackendData();

            if (bd->FontTexture != 0)
            {
                GL.DeleteTexture(bd->FontTexture);
                io.Fonts.SetTexID(0);
                bd->FontTexture = 0;
            }
        }

        static bool CheckShader(int handle, string desc)
        {
            GL.GetShader(handle, ShaderParameter.CompileStatus, out int status);
            GL.GetShader(handle, ShaderParameter.InfoLogLength, out int logLength);
            if (status == 0)
            {
                Console.Error.WriteLine($"ERROR: ImguiImplOpenGL3.CheckShader: Failed to compile {desc}!");
            }
            if (logLength > 1)
            {
                string log = GL.GetShaderInfoLog(handle);
                Console.Error.WriteLine(log);
            }
            return status == 1;
        }

        static bool CheckProgram(int handle, string desc)
        {
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int status);
            GL.GetProgram(handle, GetProgramParameterName.InfoLogLength, out int logLength);
            if (status == 0)
            {
                Console.Error.WriteLine($"ERROR: ImguiImplOpenGL3.CheckProgram: Failed to link {desc}!");
            }
            if (logLength > 1)
            {
                string log = GL.GetProgramInfoLog(handle);
                Console.Error.WriteLine(log);
            }
            return status == 1;
        }

        static void CreateDeviceObjects()
        {
            RendererData* bd = GetBackendData();

            int last_texture = GL.GetInteger(GetPName.TextureBinding2D);
            int last_array_buffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int last_pixel_unpack_buffer = GL.GetInteger(GetPName.PixelUnpackBufferBinding);
            int last_vertex_array = GL.GetInteger(GetPName.VertexArray);

            string vertex_shader_glsl_120 =
                """
                uniform mat4 ProjMtx;
                attribute vec2 Position;
                attribute vec2 UV;
                attribute vec4 Color;
                varying vec2 Frag_UV;
                varying vec4 Frag_Color;
                void main()
                {
                    Frag_UV = UV;
                    Frag_Color = Color;
                    gl_Position = vec4(Position.xy,0,1) * ProjMtx;
                }
                """;

            string vertex_shader_glsl_130 =
                """
                uniform mat4 ProjMtx;
                in vec2 Position;
                in vec2 UV;
                in vec4 Color;
                out vec2 Frag_UV;
                out vec4 Frag_Color;
                void main()
                {
                    Frag_UV = UV;
                    Frag_Color = Color;
                    gl_Position = vec4(Position.xy,0,1) * ProjMtx;
                }
                """;

            string vertex_shader_glsl_300_es =
                """
                precision highp float;
                layout(location = 0) in vec2 Position;
                layout(location = 1) in vec2 UV;
                layout(location = 2) in vec4 Color;
                uniform mat4 ProjMtx;
                out vec2 Frag_UV;
                out vec4 Frag_Color;
                void main()
                {
                    Frag_UV = UV;
                    Frag_Color = Color;
                    gl_Position = vec4(Position.xy,0,1) * ProjMtx;
                }
                """;

            string vertex_shader_glsl_410_core =
                """
                layout(location = 0) in vec2 Position;
                layout(location = 1) in vec2 UV;
                layout(location = 2) in vec4 Color;
                uniform mat4 ProjMtx;
                out vec2 Frag_UV;
                out vec4 Frag_Color;
                void main()
                {
                    Frag_UV = UV;
                    Frag_Color = Color;
                    gl_Position = vec4(Position.xy,0,1) * ProjMtx;
                }
                """;

            string fragment_shader_glsl_120 =
                """
                #ifdef GL_ES
                    precision mediump float;
                #endif
                uniform sampler2D Texture;
                varying vec2 Frag_UV;
                varying vec4 Frag_Color;
                void main()
                {
                    gl_FragColor = Frag_Color * texture2D(Texture, Frag_UV.st);
                }
                """;

            string fragment_shader_glsl_130 =
                """
                uniform sampler2D Texture;
                in vec2 Frag_UV;
                in vec4 Frag_Color;
                out vec4 Out_Color;
                void main()
                {
                    Out_Color = Frag_Color * texture2D(Texture, Frag_UV.st);
                }
                """;

            string fragment_shader_glsl_300_es =
                """
                precision mediump float;
                uniform sampler2D Texture;
                in vec2 Frag_UV;
                in vec4 Frag_Color;
                layout(location = 0) out vec4 Out_Color;
                void main()
                {
                    Out_Color = Frag_Color * texture2D(Texture, Frag_UV.st);
                }
                """;

            string fragment_shader_glsl_410_core =
                """
                in vec2 Frag_UV;
                in vec4 Frag_Color;
                uniform sampler2D Texture;
                layout(location = 0) out vec4 Out_Color;
                void main()
                {
                    Out_Color = Frag_Color * texture2D(Texture, Frag_UV.st);
                }
                """;

            string vertex_shader;
            string fragment_shader;
            if (bd->GlslVersion < 130)
            {
                vertex_shader = vertex_shader_glsl_120;
                fragment_shader = fragment_shader_glsl_120;
            }
            else if (bd->GlslVersion >= 410)
            {
                vertex_shader = vertex_shader_glsl_410_core;
                fragment_shader = fragment_shader_glsl_410_core;
            }
            else if (bd->GlslVersion == 300)
            {
                vertex_shader = vertex_shader_glsl_300_es;
                fragment_shader = fragment_shader_glsl_300_es;
            }
            else
            {
                vertex_shader = vertex_shader_glsl_130;
                fragment_shader = fragment_shader_glsl_130;
            }

            vertex_shader = vertex_shader.Insert(0, $"#version {bd->GlslVersion}{Environment.NewLine}");
            fragment_shader = fragment_shader.Insert(0, $"#version {bd->GlslVersion}{Environment.NewLine}");

            int vert = GL.CreateShader(ShaderType.VertexShader);
            // FIXME: Version string...
            GL.ShaderSource(vert, vertex_shader);
            GL.CompileShader(vert);
            CheckShader(vert, "vertex shader");

            int frag = GL.CreateShader(ShaderType.FragmentShader);
            // FIXME: Version string...
            GL.ShaderSource(frag, fragment_shader);
            GL.CompileShader(frag);
            CheckShader(frag, "fragment shader");

            bd->ShaderHandle = GL.CreateProgram();
            GL.AttachShader(bd->ShaderHandle, vert);
            GL.AttachShader(bd->ShaderHandle, frag);
            GL.LinkProgram(bd->ShaderHandle);
            CheckProgram(bd->ShaderHandle, "shader program");

            GL.DetachShader(bd->ShaderHandle, vert);
            GL.DetachShader(bd->ShaderHandle, frag);
            GL.DeleteShader(vert);
            GL.DeleteShader(frag);

            bd->UniformLocationTex = GL.GetUniformLocation(bd->ShaderHandle, "Texture");
            bd->UniformLocationProjMtx = GL.GetUniformLocation(bd->ShaderHandle, "ProjMtx");
            bd->AttribLocationVtxPos = GL.GetAttribLocation(bd->ShaderHandle, "Position");
            bd->AttribLocationVtxUV = GL.GetAttribLocation(bd->ShaderHandle, "UV");
            bd->AttribLocationVtxColor = GL.GetAttribLocation(bd->ShaderHandle, "Color");

            bd->VboHandle = GL.GenBuffer();
            bd->EboHandle = GL.GenBuffer();

            CreateFontsTexture();

            GL.BindTexture(TextureTarget.Texture2D, last_texture);
            GL.BindBuffer(BufferTarget.ArrayBuffer, last_array_buffer);
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, last_pixel_unpack_buffer);
            GL.BindVertexArray(last_vertex_array);
        }

        static void DestroyDeviceObjects()
        {
            RendererData* bd = GetBackendData();

            if (bd->VboHandle != 0)
            {
                GL.DeleteBuffer(bd->VboHandle);
                bd->VboHandle = 0;
            }

            if (bd->EboHandle != 0)
            {
                GL.DeleteBuffer(bd->EboHandle);
                bd->EboHandle = 0;
            }

            if (bd->ShaderHandle != 0)
            {
                GL.DeleteProgram(bd->ShaderHandle);
                bd->ShaderHandle = 0;
            }

            DestroyFontsTexture();
        }

        static void InitMultiViewportSupport()
        {
            var platformIO = ImGui.GetPlatformIO();
            platformIO.Renderer_RenderWindow = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Renderer_RenderWindow;
        }

        static void ShutdownMultiViewportSupport()
        {
            ImGui.DestroyPlatformWindows();
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Renderer_RenderWindow(ImGuiViewportPtr viewport)
        {
            if (viewport.Flags.HasFlag(ImGuiViewportFlags.NoRendererClear))
            {
                GL.ClearColor(Color4.Black);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
            RenderDrawData(viewport.DrawData);
        }
    }
}
