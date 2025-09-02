using ImGui_OpenTK.Backends;
using ImGuiNET;
using OpenTK.Core.Utility;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using System;

namespace PAL2_OpenGL_Sample
{
    internal class Program
    {
        static WindowHandle Window;

        static void Main(string[] args)
        {
            ToolkitOptions options = new ToolkitOptions()
            {
                ApplicationName = "OpenTK ImGui PAL2 OpenGL Example",
                Logger = new ConsoleLogger(),
                FeatureFlags = ToolkitFlags.EnableOpenGL,
            };

            EventQueue.EventRaised += EventQueue_EventRaised;

            Toolkit.Init(options);

            OpenGLGraphicsApiHints openglSettings = new OpenGLGraphicsApiHints()
            {
                Version = new Version(4, 1),
            };

            Window = Toolkit.Window.Create(openglSettings);
            Toolkit.Window.SetSize(Window, (1280, 720));
            Toolkit.Window.SetBorderStyle(Window, WindowBorderStyle.ResizableBorder);
            Toolkit.Window.SetTitle(Window, "OpenTK ImGui PAL2 OpenGL demo");

            OpenGLContextHandle glcontext = Toolkit.OpenGL.CreateFromWindow(Window);
            Toolkit.OpenGL.SetCurrentContext(glcontext);
            Toolkit.OpenGL.SetSwapInterval(1);
            GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(glcontext));

            Toolkit.Window.SetMode(Window, WindowMode.Normal);

            ImGui.CreateContext();
            ImGuiIOPtr io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

            ImGui.StyleColorsDark();

            ImGuiStylePtr style = ImGui.GetStyle();
            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                style.WindowRounding = 0.0f;
                style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
            }

            ImguiImplOpenTKPAL2.Init(Window, glcontext);
            ImguiImplOpenGL3.Init();

            while (true)
            {
                Toolkit.Window.ProcessEvents(false);
                if (Toolkit.Window.IsWindowDestroyed(Window))
                {
                    break;
                }

                ImguiImplOpenGL3.NewFrame();
                ImguiImplOpenTKPAL2.NewFrame();
                ImGui.NewFrame();

                ImGui.DockSpaceOverViewport();

                ImGui.ShowDemoWindow();

                ImGui.Render();
                Toolkit.Window.GetFramebufferSize(Window, out Vector2i fbSize);
                GL.Viewport(0, 0, fbSize.X, fbSize.Y);
                GL.ClearColor(new Color4<Rgba>(0, 32, 48, 255));
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

                if (ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
                {
                    ImGui.UpdatePlatformWindows();
                    ImGui.RenderPlatformWindowsDefault();
                    Toolkit.OpenGL.SetCurrentContext(glcontext);
                }

                Toolkit.OpenGL.SwapBuffers(glcontext);
            }

            ImguiImplOpenGL3.Shutdown();
            ImguiImplOpenTKPAL2.Shutdown();
        }

        private static void EventQueue_EventRaised(PalHandle? handle, PlatformEventType type, EventArgs args)
        {
            if (args is CloseEventArgs close)
            {
                if (close.Window == Window)
                {
                    Toolkit.Window.Destroy(Window);
                }
            }
        }
    }
}
