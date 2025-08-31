using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SNVector2 = System.Numerics.Vector2;

namespace Dear_ImGui_Sample.Backends
{
    internal unsafe static class ImguiImplOpenTK4
    {
        struct BackendData
        {
            public nint Context;
            public IntPtr WindowPtr;

            public long Time;

            public Vector2 LastValidMousePos;

            public bool WantUpdateMonitors;
        }

        class WindowCallbacks
        {
            public NativeWindow Window;

            public WindowCallbacks(NativeWindow window)
            {
                Window = window;
            }

            public void Window_MouseButton(MouseButtonEventArgs e)
            {
                var io = ImGui.GetIO();
                
                UpdateKeyModifiers(io, Window);

                int button = (int)e.Button;
                if (button >= 0 && button <= (int)ImGuiMouseButton.COUNT)
                {
                    io.AddMouseButtonEvent((int)e.Button, e.IsPressed);
                }
            }

            public void Window_MouseWheel(MouseWheelEventArgs e)
            {
                var io = ImGui.GetIO();
                io.AddMouseWheelEvent(e.OffsetX, e.OffsetY);
            }

            public void Window_KeyUp(KeyboardKeyEventArgs e) => Window_Key(e, false);
            public void Window_KeyDown(KeyboardKeyEventArgs e) => Window_Key(e, true);

            public void Window_Key(KeyboardKeyEventArgs e, bool isPressed)
            {
                var io = ImGui.GetIO();
                
                UpdateKeyModifiers(io, Window);

                ImGuiKey imguiKey = TranslateKey(e.Key);
                io.AddKeyEvent(imguiKey, isPressed);
                io.SetKeyEventNativeData(imguiKey, (int)e.Key, e.ScanCode);
            }

            public void Window_FocusedChanged(FocusedChangedEventArgs e)
            {
                var io = ImGui.GetIO();
                io.AddFocusEvent(e.IsFocused);
            }

            public void Window_MouseMove(MouseMoveEventArgs e)
            {
                var io = ImGui.GetIO();
                BackendData* bd = GetBackendData();

                float x = e.X;
                float y = e.Y;

                if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
                {
                    var clientLocation = Window.ClientLocation;
                    x += clientLocation.X;
                    y += clientLocation.Y;
                }

                io.AddMousePosEvent(x, y);
                bd->LastValidMousePos = new(x, y);
            }

            public void Window_MouseEnter()
            {
                var io = ImGui.GetIO();
                BackendData* bd = GetBackendData();

                io.AddMousePosEvent(bd->LastValidMousePos.X, bd->LastValidMousePos.Y);
            }

            public void Window_MouseLeave()
            {
                var io = ImGui.GetIO();
                BackendData* bd = GetBackendData();

                bd->LastValidMousePos = new(io.MousePos.X, io.MousePos.Y);
                io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
            }

            public void Window_TextInput(TextInputEventArgs e)
            {
                var io = ImGui.GetIO();

                io.AddInputCharacter((uint)e.Unicode);
            }
        }

        static readonly Dictionary<IntPtr, NativeWindow> WindowMap = new Dictionary<nint, NativeWindow>();
        static readonly Dictionary<NativeWindow, WindowCallbacks> CallbackMap = new Dictionary<NativeWindow, WindowCallbacks>();

        private static BackendData* GetBackendData()
        {
            return ImGui.GetCurrentContext() == 0 ? null : (BackendData*)ImGui.GetIO().BackendPlatformUserData;
        }

        public static ImGuiKey TranslateKey(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9)
                return key - Keys.D0 + ImGuiKey._0;

            if (key >= Keys.A && key <= Keys.Z)
                return key - Keys.A + ImGuiKey.A;

            if (key >= Keys.KeyPad0 && key <= Keys.KeyPad9)
                return key - Keys.KeyPad0 + ImGuiKey.Keypad0;

            if (key >= Keys.F1 && key <= Keys.F24)
                return key - Keys.F1 + ImGuiKey.F24;

            switch (key)
            {
                case Keys.Tab: return ImGuiKey.Tab;
                case Keys.Left: return ImGuiKey.LeftArrow;
                case Keys.Right: return ImGuiKey.RightArrow;
                case Keys.Up: return ImGuiKey.UpArrow;
                case Keys.Down: return ImGuiKey.DownArrow;
                case Keys.PageUp: return ImGuiKey.PageUp;
                case Keys.PageDown: return ImGuiKey.PageDown;
                case Keys.Home: return ImGuiKey.Home;
                case Keys.End: return ImGuiKey.End;
                case Keys.Insert: return ImGuiKey.Insert;
                case Keys.Delete: return ImGuiKey.Delete;
                case Keys.Backspace: return ImGuiKey.Backspace;
                case Keys.Space: return ImGuiKey.Space;
                case Keys.Enter: return ImGuiKey.Enter;
                case Keys.Escape: return ImGuiKey.Escape;
                case Keys.Apostrophe: return ImGuiKey.Apostrophe;
                case Keys.Comma: return ImGuiKey.Comma;
                case Keys.Minus: return ImGuiKey.Minus;
                case Keys.Period: return ImGuiKey.Period;
                case Keys.Slash: return ImGuiKey.Slash;
                case Keys.Semicolon: return ImGuiKey.Semicolon;
                case Keys.Equal: return ImGuiKey.Equal;
                case Keys.LeftBracket: return ImGuiKey.LeftBracket;
                case Keys.Backslash: return ImGuiKey.Backslash;
                case Keys.RightBracket: return ImGuiKey.RightBracket;
                case Keys.GraveAccent: return ImGuiKey.GraveAccent;
                case Keys.CapsLock: return ImGuiKey.CapsLock;
                case Keys.ScrollLock: return ImGuiKey.ScrollLock;
                case Keys.NumLock: return ImGuiKey.NumLock;
                case Keys.PrintScreen: return ImGuiKey.PrintScreen;
                case Keys.Pause: return ImGuiKey.Pause;
                case Keys.KeyPadDecimal: return ImGuiKey.KeypadDecimal;
                case Keys.KeyPadDivide: return ImGuiKey.KeypadDivide;
                case Keys.KeyPadMultiply: return ImGuiKey.KeypadMultiply;
                case Keys.KeyPadSubtract: return ImGuiKey.KeypadSubtract;
                case Keys.KeyPadAdd: return ImGuiKey.KeypadAdd;
                case Keys.KeyPadEnter: return ImGuiKey.KeypadEnter;
                case Keys.KeyPadEqual: return ImGuiKey.KeypadEqual;
                case Keys.LeftShift: return ImGuiKey.LeftShift;
                case Keys.LeftControl: return ImGuiKey.LeftCtrl;
                case Keys.LeftAlt: return ImGuiKey.LeftAlt;
                case Keys.LeftSuper: return ImGuiKey.LeftSuper;
                case Keys.RightShift: return ImGuiKey.RightShift;
                case Keys.RightControl: return ImGuiKey.RightCtrl;
                case Keys.RightAlt: return ImGuiKey.RightAlt;
                case Keys.RightSuper: return ImGuiKey.RightSuper;
                case Keys.Menu: return ImGuiKey.Menu;
                default: return ImGuiKey.None;
            }
        }

        private static void UpdateKeyModifiers(ImGuiIOPtr io, NativeWindow window)
        {
            io.AddKeyEvent(ImGuiKey.ModCtrl, window.KeyboardState.IsKeyDown(Keys.LeftControl) || window.KeyboardState.IsKeyDown(Keys.RightControl));
            io.AddKeyEvent(ImGuiKey.ModShift, window.KeyboardState.IsKeyDown(Keys.LeftShift) || window.KeyboardState.IsKeyDown(Keys.RightShift));
            io.AddKeyEvent(ImGuiKey.ModAlt, window.KeyboardState.IsKeyDown(Keys.LeftAlt) || window.KeyboardState.IsKeyDown(Keys.RightAlt));
            io.AddKeyEvent(ImGuiKey.ModSuper, window.KeyboardState.IsKeyDown(Keys.LeftSuper) || window.KeyboardState.IsKeyDown(Keys.RightSuper));
        }

        private static void Monitors_OnMonitorConnected(MonitorEventArgs e)
        {
            var io = ImGui.GetIO();
            BackendData* bd = GetBackendData();

            bd->WantUpdateMonitors = true;
        }

        static void InstallCallbacks(NativeWindow window)
        {
            WindowCallbacks callbacks = new WindowCallbacks(window);

            window.MouseDown += callbacks.Window_MouseButton;
            window.MouseUp += callbacks.Window_MouseButton;
            window.MouseWheel += callbacks.Window_MouseWheel;
            window.KeyUp += callbacks.Window_KeyUp;
            window.KeyDown += callbacks.Window_KeyDown;
            window.FocusedChanged += callbacks.Window_FocusedChanged;
            window.MouseMove += callbacks.Window_MouseMove;
            window.MouseEnter += callbacks.Window_MouseEnter;
            window.MouseLeave += callbacks.Window_MouseLeave;
            window.TextInput += callbacks.Window_TextInput;

            CallbackMap.Add(window, callbacks);
        }

        static void RestoreCallbacks(NativeWindow window)
        {
            WindowCallbacks callbacks = CallbackMap[window];

            window.MouseDown -= callbacks.Window_MouseButton;
            window.MouseUp -= callbacks.Window_MouseButton;
            window.MouseWheel -= callbacks.Window_MouseWheel;
            window.KeyUp -= callbacks.Window_KeyUp;
            window.KeyDown -= callbacks.Window_KeyDown;
            window.FocusedChanged -= callbacks.Window_FocusedChanged;
            window.MouseMove -= callbacks.Window_MouseMove;
            window.MouseEnter -= callbacks.Window_MouseEnter;
            window.MouseLeave -= callbacks.Window_MouseLeave;
            window.TextInput -= callbacks.Window_TextInput;

            CallbackMap.Remove(window);
        }

        public static bool Init(NativeWindow window)
        {
            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
            io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
            io.BackendFlags |= ImGuiBackendFlags.HasMouseHoveredViewport;
            
            BackendData* bd = (BackendData*)NativeMemory.AllocZeroed((uint)sizeof(BackendData));
            io.BackendPlatformUserData = (IntPtr)bd;
            io.NativePtr->BackendPlatformName = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("opentk_impl_opentk4"u8));
            WindowMap.Add((IntPtr)window.WindowPtr, window);

            bd->Context = ImGui.GetCurrentContext();
            bd->WindowPtr = (IntPtr)window.WindowPtr;
            bd->WantUpdateMonitors = true;

            var platformIO = ImGui.GetPlatformIO();
            platformIO.NativePtr->Platform_SetClipboardTextFn = (IntPtr)(delegate* unmanaged[Cdecl]<nint, byte*, void>)(&Platform_SetClipboardText);
            platformIO.NativePtr->Platform_GetClipboardTextFn = (IntPtr)(delegate* unmanaged[Cdecl]<nint, byte*>)(&Platform_GetClipboardText);

            platformIO.NativePtr->Monitors = default;

            InstallCallbacks(window);

            UpdateMonitors();
            Monitors.OnMonitorConnected += Monitors_OnMonitorConnected;

            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
            mainViewport.PlatformHandle = (IntPtr)window.WindowPtr;

            InitMultiViewportSupport();

            return true;
        }

        public static void Shutdown()
        {
            BackendData* bd = GetBackendData();
            var io = ImGui.GetIO();

            ShutdownMultiViewportSupport();

            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();

            io.NativePtr->BackendPlatformName = null;
            io.BackendPlatformUserData = 0;
            io.BackendFlags &= ~(ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos | ImGuiBackendFlags.HasGamepad);
            if (WindowMap.TryGetValue(bd->WindowPtr, out NativeWindow window))
            {
                RestoreCallbacks(window);
            }
            WindowMap.Remove(bd->WindowPtr);

            Monitors.OnMonitorConnected -= Monitors_OnMonitorConnected;

            NativeMemory.Free(bd);
        }

        static void UpdateMouseData()
        {
            var io = ImGui.GetIO();
            var platformIO = ImGui.GetPlatformIO();
            BackendData* bd = GetBackendData();

            uint mouse_viewport_id = 0;
            Vector2 prevMousePos = new(io.MousePos.X, io.MousePos.Y);

            for (int n = 0; n < platformIO.Viewports.Size; n++)
            {
                ImGuiViewportPtr viewport = platformIO.Viewports[n];
                nint windowPtr = viewport.PlatformHandle;
                // FIXME:
                if (windowPtr == 0)
                    continue;
                NativeWindow window = WindowMap[windowPtr];

                if (window.IsFocused)
                {
                    if (io.WantSetMousePos)
                    {
                        window.MousePosition = new Vector2(prevMousePos.X - viewport.Pos.X, prevMousePos.Y - viewport.Pos.Y);
                    }
                }

                bool noInput = (viewport.Flags & ImGuiViewportFlags.NoInputs) != 0;
                window.MousePassthrough = noInput;

                if (GLFW.GetWindowAttrib(window.WindowPtr, WindowAttributeGetBool.Hovered))
                    mouse_viewport_id = viewport.ID;
            }

            if ((io.BackendFlags & ImGuiBackendFlags.HasMouseHoveredViewport) != 0)
            {
                io.AddMouseViewportEvent(mouse_viewport_id);
            }
        }

        static void UpdateMouseCursor()
        {
            var io = ImGui.GetIO();
            var platformIO = ImGui.GetPlatformIO();
            BackendData* bd = GetBackendData();

            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0 || WindowMap[bd->WindowPtr].CursorState == CursorState.Grabbed)
                return;

            ImGuiMouseCursor imguiCursor = ImGui.GetMouseCursor();
            for (int n = 0; n < platformIO.Viewports.Size; n++)
            {
                if (platformIO.Viewports[n].PlatformHandle == 0)
                    continue;

                NativeWindow window = WindowMap[platformIO.Viewports[n].PlatformHandle];
                if (imguiCursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
                {
                    window.CursorState = CursorState.Hidden;
                }
                else
                {
                    window.Cursor = GetCursor(imguiCursor);
                    window.CursorState = CursorState.Normal;
                }
            }

            static MouseCursor GetCursor(ImGuiMouseCursor imguiCursor)
            {
                switch (imguiCursor)
                {
                    case ImGuiMouseCursor.None:
                        return MouseCursor.Empty;
                    case ImGuiMouseCursor.Arrow:
                        return MouseCursor.Default;
                    case ImGuiMouseCursor.TextInput:
                        return MouseCursor.IBeam;
                    case ImGuiMouseCursor.ResizeAll:
                        return MouseCursor.ResizeAll;
                    case ImGuiMouseCursor.ResizeNS:
                        return MouseCursor.ResizeNS;
                    case ImGuiMouseCursor.ResizeEW:
                        return MouseCursor.ResizeEW;
                    case ImGuiMouseCursor.ResizeNESW:
                        return MouseCursor.ResizeNESW;
                    case ImGuiMouseCursor.ResizeNWSE:
                        return MouseCursor.ResizeNWSE;
                    case ImGuiMouseCursor.Hand:
                        return MouseCursor.PointingHand;
                    case ImGuiMouseCursor.NotAllowed:
                        return MouseCursor.NotAllowed;
                    default:
                        return MouseCursor.Default;
                }
            }
        }

        static void UpdateMonitors()
        {
            var io = ImGui.GetIO();
            var platformIO = ImGui.GetPlatformIO();
            BackendData* bd = GetBackendData();

            bd->WantUpdateMonitors = false;

            List<MonitorInfo> monitors = Monitors.GetMonitors();
            if (monitors.Count == 0)
                return;

            if (platformIO.NativePtr->Monitors.Data != 0)
                Marshal.FreeHGlobal(platformIO.NativePtr->Monitors.Data);
            platformIO.NativePtr->Monitors = new ImVector(monitors.Count, monitors.Count, (IntPtr)Marshal.AllocHGlobal(monitors.Count * sizeof(ImGuiPlatformMonitor)));
            NativeMemory.Clear((void*)platformIO.NativePtr->Monitors.Data, (nuint)(platformIO.NativePtr->Monitors.Capacity * sizeof(ImGuiPlatformMonitor)));
            for (int i = 0; i < monitors.Count; i++)
            {
                ref ImGuiPlatformMonitor monitor = ref Unsafe.Add(ref Unsafe.AsRef<ImGuiPlatformMonitor>((void*)platformIO.Monitors.Data), i);

                var mode = monitors[i].CurrentVideoMode;
                var clientArea = monitors[i].ClientArea;
                monitor.MainPos = new(clientArea.Min.X, clientArea.Min.Y);
                monitor.MainSize = new(clientArea.Size.X, clientArea.Size.Y);

                var workArea = monitors[i].WorkArea;
                monitor.WorkPos = new(workArea.Min.X, workArea.Min.Y);
                monitor.WorkSize = new(workArea.Size.X, workArea.Size.Y);
                monitor.DpiScale = monitors[i].HorizontalScale;
                monitor.PlatformHandle = (void*)monitors[i].Handle.Pointer;
            }
        }

        public static void NewFrame()
        {
            var io = ImGui.GetIO();
            BackendData* bd = GetBackendData();

            NativeWindow window = WindowMap[bd->WindowPtr];
            Vector2 clientSize = window.ClientSize;
            Vector2 fbSize = window.FramebufferSize;
            io.DisplaySize = new(fbSize.X, fbSize.Y);
            if (fbSize.X > 0 && fbSize.Y > 0)
            {
                io.DisplayFramebufferScale = new(clientSize.X / fbSize.X, clientSize.Y / fbSize.Y);
            }
            if (bd->WantUpdateMonitors)
            {
                UpdateMonitors();
            }

            var currentTime = Stopwatch.GetTimestamp();
            if (currentTime <= bd->Time)
            {
                currentTime = bd->Time + 1;
            }
            io.DeltaTime = bd->Time > 0.0 ? ((currentTime - bd->Time) / (float)Stopwatch.Frequency) : (1.0f / 60.0f);
            bd->Time = currentTime;

            UpdateMouseData();
            UpdateMouseCursor();
            // UpdateGamepads()
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetClipboardText(nint ctx, byte* text)
        {
            Marshal.PtrToStringUTF8((IntPtr)text);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte* Platform_GetClipboardText(nint ctx)
        {
            return default;
        }

        struct ViewportData
        {
            public nint WindowPtr;
            public bool WindowOwned;
            public int IgnoreWindowPosEventFrame;
            public int IgnoreWindowSizeEventFrame;
        }

        static void InitMultiViewportSupport()
        {
            var platformIO = ImGui.GetPlatformIO();
            BackendData* bd = GetBackendData();

            //var a0 = (ImGuiNET.Platform_CreateWindow)Platform_CreateWindow;
            //var a1 = (ImGuiNET.Platform_DestroyWindow)Platform_DestroyWindow;
            //var a2 = (ImGuiNET.Platform_ShowWindow)Platform_ShowWindow;
            //var a3 = (ImGuiNET.Platform_GetWindowPos)Platform_GetWindowPos;
            //var a4 = (ImGuiNET.Platform_SetWindowPos)Platform_SetWindowPos;
            //var a5 = (ImGuiNET.Platform_GetWindowSize)Platform_GetWindowSize;
            //var a6 = (ImGuiNET.Platform_SetWindowSize)Platform_SetWindowSize;
            //var a7 = (ImGuiNET.Platform_SetWindowTitle)Platform_SetWindowTitle;
            //var a8 = (ImGuiNET.Platform_SetWindowFocus)Platform_SetWindowFocus;
            //var a9 = (ImGuiNET.Platform_GetWindowFocus)Platform_GetWindowFocus;
            //var a10 = (ImGuiNET.Platform_GetWindowMinimized)Platform_GetWindowMinimized;
            //var a11 = (ImGuiNET.Platform_SetWindowAlpha)Platform_SetWindowAlpha;
            //var a12 = (ImGuiNET.Platform_RenderWindow)Platform_RenderWindow;
            //var a13 = (ImGuiNET.Platform_SwapBuffers)Platform_SwapBuffers;

            platformIO.Platform_CreateWindow = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_CreateWindow;
            platformIO.Platform_DestroyWindow = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_DestroyWindow;
            platformIO.Platform_ShowWindow = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_ShowWindow;
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowPos(platformIO, (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2 *, void>)&Platform_GetWindowPos);
            platformIO.Platform_SetWindowPos = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2, void>)&Platform_SetWindowPos;
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowSize(platformIO, (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2*, void>)&Platform_GetWindowSize);
            platformIO.Platform_SetWindowSize = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2, void>)&Platform_SetWindowSize;
            platformIO.Platform_SetWindowTitle = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, nint, void>)&Platform_SetWindowTitle;
            platformIO.Platform_SetWindowFocus = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_SetWindowFocus;
            platformIO.Platform_GetWindowFocus = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, byte>)&Platform_GetWindowFocus;
            platformIO.Platform_GetWindowMinimized = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, byte>)&Platform_GetWindowMinimized;
            platformIO.Platform_SetWindowAlpha = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, float, void>)&Platform_SetWindowAlpha;
            platformIO.Platform_RenderWindow = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void*, void>)&Platform_RenderWindow;
            platformIO.Platform_SwapBuffers = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void*, void>)&Platform_SwapBuffers;
            
            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
            ViewportData* vd = (ViewportData*)NativeMemory.AllocZeroed((uint)sizeof(ViewportData));
            vd->WindowPtr = bd->WindowPtr;
            vd->WindowOwned = false;
            mainViewport.PlatformUserData = (IntPtr)vd;
            mainViewport.PlatformHandle = bd->WindowPtr;
        }

        static void ShutdownMultiViewportSupport()
        {
            ImGui.DestroyPlatformWindows();
        }

        private static void Window_Resize(ResizeEventArgs e)
        {
            // FIXME: Get the platform handle...?
        }

        private static void Window_Move(WindowPositionEventArgs obj)
        {
            // FIXME: Get the platform handle...?
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_CreateWindow(ImGuiViewportPtr viewport)
        {
            BackendData* bd = GetBackendData();
            NativeWindow mainWindow = WindowMap[bd->WindowPtr];

            ViewportData* vd = (ViewportData*)NativeMemory.AllocZeroed((uint)sizeof(ViewportData));
            viewport.PlatformUserData = (IntPtr)vd;

            // FIXME??
            GLFW.WindowHint(WindowHintBool.FocusOnShow, false);
            GLFW.WindowHint(WindowHintBool.Floating, viewport.Flags.HasFlag(ImGuiViewportFlags.TopMost) ? true : false);
            NativeWindow window = new NativeWindow(new NativeWindowSettings()
            {
                StartVisible = false,
                StartFocused = false,
                WindowBorder = viewport.Flags.HasFlag(ImGuiViewportFlags.NoDecoration) ? WindowBorder.Hidden : WindowBorder.Resizable,
                SharedContext = mainWindow.Context,
                Title = "No Title Yet",
            });
            WindowMap.Add((IntPtr)window.WindowPtr, window);

            vd->WindowPtr = (IntPtr)window.WindowPtr;
            vd->WindowOwned = true;
            viewport.PlatformHandle = vd->WindowPtr;

            window.ClientLocation = new((int)viewport.Pos.X, (int)viewport.Pos.Y);

            InstallCallbacks(window);
            window.Move += Window_Move;
            window.Resize += Window_Resize;

            if (window.API == ContextAPI.OpenGL)
            {
                window.MakeCurrent();
                window.VSync = VSyncMode.Off;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_DestroyWindow(ImGuiViewportPtr viewport)
        {
            BackendData* bd = GetBackendData();
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            if (vd != null)
            {
                if (vd->WindowOwned)
                {
                    NativeWindow window = WindowMap[vd->WindowPtr];
                    window.Dispose();

                    WindowMap.Remove(vd->WindowPtr);
                }
                vd->WindowPtr = 0;
                NativeMemory.Free(vd);
            }
            viewport.PlatformUserData = viewport.PlatformHandle = 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_ShowWindow(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            window.IsVisible = true;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_GetWindowPos(ImGuiViewportPtr viewport, SNVector2* outPos)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            if (WindowMap.TryGetValue(vd->WindowPtr, out NativeWindow window))
            {
                *outPos = new(window.ClientLocation.X, window.ClientLocation.Y);
            }
            else
            {
                *outPos = default;
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowPos(ImGuiViewportPtr viewport, SNVector2 pos)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            window.ClientLocation = new((int)pos.X, (int)pos.Y);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_GetWindowSize(ImGuiViewportPtr viewport, SNVector2* outPos)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            *outPos = new(window.ClientSize.X, window.ClientSize.Y);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowSize(ImGuiViewportPtr viewport, SNVector2 size)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            window.ClientSize = new((int)size.X, (int)size.Y);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowTitle(ImGuiViewportPtr viewport, nint name)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            window.Title = Marshal.PtrToStringUTF8(name);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowFocus(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            window.Focus();
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte Platform_GetWindowFocus(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            return window.IsFocused ? (byte)1 : (byte)0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte Platform_GetWindowMinimized(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            return window.WindowState == WindowState.Minimized ? (byte)1 : (byte)0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowAlpha(ImGuiViewportPtr viewport, float alpha)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            GLFW.SetWindowOpacity(window.WindowPtr, alpha);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_RenderWindow(ImGuiViewportPtr viewport, void* _)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            if (window.API == ContextAPI.OpenGL)
            {
                window.MakeCurrent();
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SwapBuffers(ImGuiViewportPtr viewport,  void* _)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            NativeWindow window = WindowMap[vd->WindowPtr];
            if (window.API == ContextAPI.OpenGL)
            {
                window.MakeCurrent();
                window.Context.SwapBuffers();
            }
        }
    }
}
