using Dear_ImGui_Sample;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Engine;

public class Window : GameWindow
{
	public RenderTexture bloomDownscaledRenderTexture;
	public ImGuiController imGuiController;
	public RenderTexture postProcessRenderTexture;
	public RenderTexture sceneRenderTexture;

	public Window() : base(GameWindowSettings.Default,
	                       new NativeWindowSettings
	                       {Size = new Vector2i(2560, 1600), APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core, NumberOfSamples = 8})
	{
		I = this;
		
		WindowState = WindowState.Maximized;
		//WindowState = WindowState.Fullscreen;
	}

	public static Window I { get; private set; }

	protected override void OnLoad()
	{
		Title = $"TenebrisCapulus Engine | {GL.GetString(StringName.Version)}";

		//MaterialCache.CacheAllMaterialsInProject();
		imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

		Vector2 size = new Vector2(100, 100); // temporaly 10x10 textures because we cant access Camera.I.size before Scene started-camera is a gameobject
		sceneRenderTexture = new RenderTexture(size);
		postProcessRenderTexture = new RenderTexture(size);

		Editor.I.Init();
		Scene.I.Start();
		sceneRenderTexture = new RenderTexture(Camera.I.size);
		postProcessRenderTexture = new RenderTexture(Camera.I.size);

		//bloomDownscaledRenderTexture = new RenderTexture(Camera.I.size);
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);

		// Update the opengl viewport
		//GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		// Tell ImGui of the new size
		imGuiController?.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		Debug.StartTimer("Scene Update");
		Scene.I.Update();
		Debug.EndTimer("Scene Update");

		if (Global.EditorAttached)
		{
			Editor.I.Update();
		}

		base.OnUpdateFrame(args);
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		Debug.CountStat("Draw Calls", 0);
		Debug.StartTimer("Scene Render");

		GL.ClearColor(0, 33, 0, 33);
		GL.Clear(ClearBufferMask.ColorBufferBit);

		sceneRenderTexture.Bind(); // start rendering to sceneRenderTexture
		GL.Viewport(0, 0, (int) Camera.I.size.X, (int) Camera.I.size.Y);

		GL.Enable(EnableCap.Blend);
		//GL.Enable(EnableCap.Multisample);
		Scene.I.Render();

		sceneRenderTexture.Unbind(); // end rendering to sceneRenderTexture
		GL.Disable(EnableCap.Blend);
		
		postProcessRenderTexture.Bind();
		GL.ClearColor(0, 0, 0, 0);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		// draw sceneRenderTexture.colorAttachment with post process- into postProcessRenderTexture target
		postProcessRenderTexture.Render(sceneRenderTexture.colorAttachment);
		//postProcessRenderTexture.RenderWithPostProcess(sceneRenderTexture.colorAttachment);
		//postProcessRenderTexture.RenderSnow(sceneRenderTexture.colorAttachment);

		postProcessRenderTexture.Unbind();


		imGuiController.Update(this, (float) e.Time);
		GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		imGuiController.WindowResized(ClientSize.X, ClientSize.Y);


		Editor.I.Draw();
		//GL.Enable(EnableCap.Multisample);

		imGuiController.Render();
		
		// ------------- IMGUI -------------


		SwapBuffers();
		base.OnRenderFrame(e);

		Debug.ClearTimers();
		Debug.ClearStats();
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);

		imGuiController.PressChar((char) e.Unicode);
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		imGuiController.MouseScroll(new Vector2(e.OffsetX, e.OffsetY));
	}
}