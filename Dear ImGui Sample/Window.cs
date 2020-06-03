using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Drawing;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;

namespace Dear_ImGui_Sample
{
    public class Window : GameWindow
    {
        ImGuiController _controller;

        public Window() : base(GameWindowSettings.Default, new NativeWindowSettings(){ Size = new Vector2i(1600, 900), APIVersion = new Version(4, 5)})
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        }
        
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _controller.Update(this, (float)e.Time);

            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            ImGui.ShowDemoWindow();

            _controller.Render();

            Util.CheckGLError("End of frame");

            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            
            
            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            
            _controller.MouseScroll(e.Offset);
        }
    }
}
