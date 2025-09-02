using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SNVector2 = System.Numerics.Vector2;

namespace ImGui_OpenTK.Backends
{
    public unsafe static class ImguiImplOpenTKPAL2
    {
        struct BackendData
        {
            public nint Context;
            public nint WindowID;

            public long Time;

            public Vector2 LastValidMousePos;

            public bool WantUpdateMonitors;

            public nint NativeClipboardText;

            public nint MouseWindowID;

            public SystemCursorType CurrentCursorType;

            public int MousePendingLeaveFrame;
        }

        struct WindowInfo
        {
            public WindowHandle Handle;
            public OpenGLContextHandle GLContext;
            public nint ID;

            public WindowInfo(WindowHandle handle, OpenGLContextHandle glContext, nint iD)
            {
                Handle = handle;
                GLContext = glContext;
                ID = iD;
            }
        }

        static readonly List<WindowInfo> Windows = new List<WindowInfo>();
        static readonly Queue<nint> WindowIDFreelist = new Queue<nint>();
        static nint NextFreeID = 1;
        static nint RegisterWindow(WindowHandle handle, OpenGLContextHandle glContext)
        {
            nint id = AllocateWindowID();
            Windows.Add(new WindowInfo(handle, glContext, id));
            return id;

            static nint AllocateWindowID()
            {
                if (WindowIDFreelist.TryDequeue(out nint ID))
                {
                    return ID;
                }
                else
                {
                    return NextFreeID++;
                }
            }
        }
        static WindowHandle GetWindowFromID(nint id)
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                if (Windows[i].ID == id)
                {
                    return Windows[i].Handle;
                }
            }

            throw new KeyNotFoundException($"Could not find window with id: {id}");
        }
        static OpenGLContextHandle GetContextFromID(nint id)
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                if (Windows[i].ID == id)
                {
                    return Windows[i].GLContext;
                }
            }

            throw new KeyNotFoundException($"Could not find window with id: {id}");
        }
        static bool TryGetWindowFromID(nint id, [NotNullWhen(true)] out WindowHandle window)
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                if (Windows[i].ID == id)
                {
                    window = Windows[i].Handle;
                    return true;
                }
            }
            window = null;
            return false;
        }
        static bool TryGetContextFromID(nint id, out OpenGLContextHandle glContext)
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                if (Windows[i].ID == id)
                {
                    glContext = Windows[i].GLContext;
                    return true;
                }
            }
            glContext = null;
            return false;
        }
        static bool TryGetIDFromWindow(WindowHandle window, [NotNullWhen(true)] out nint id)
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                if (Windows[i].Handle == window)
                {
                    id = Windows[i].ID;
                    return true;
                }
            }
            id = -1;
            return false;
        }
        static void FreeWindow(WindowHandle handle)
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                if (Windows[i].Handle == handle)
                {
                    nint id = Windows[i].ID;
                    Windows.RemoveAt(i);
                    WindowIDFreelist.Enqueue(id);
                    break;
                }
            }
        }

        private static BackendData* GetBackendData()
        {
            return ImGui.GetCurrentContext() == 0 ? null : (BackendData*)ImGui.GetIO().BackendPlatformUserData;
        }

        public static ImGuiKey TranslateKey(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9)
                return key - Key.D0 + ImGuiKey._0;

            if (key >= Key.A && key <= Key.Z)
                return key - Key.A + ImGuiKey.A;

            if (key >= Key.Keypad0 && key <= Key.Keypad9)
                return key - Key.Keypad0 + ImGuiKey.Keypad0;

            if (key >= Key.F1 && key <= Key.F24)
                return key - Key.F1 + ImGuiKey.F24;

            switch (key)
            {
                case Key.Tab: return ImGuiKey.Tab;
                case Key.LeftArrow: return ImGuiKey.LeftArrow;
                case Key.RightArrow: return ImGuiKey.RightArrow;
                case Key.UpArrow: return ImGuiKey.UpArrow;
                case Key.DownArrow: return ImGuiKey.DownArrow;
                case Key.PageUp: return ImGuiKey.PageUp;
                case Key.PageDown: return ImGuiKey.PageDown;
                case Key.Home: return ImGuiKey.Home;
                case Key.End: return ImGuiKey.End;
                case Key.Insert: return ImGuiKey.Insert;
                case Key.Delete: return ImGuiKey.Delete;
                case Key.Backspace: return ImGuiKey.Backspace;
                case Key.Space: return ImGuiKey.Space;
                case Key.Return: return ImGuiKey.Enter;
                case Key.Escape: return ImGuiKey.Escape;
                case Key.OEM7: return ImGuiKey.Apostrophe;
                case Key.Comma: return ImGuiKey.Comma;
                case Key.Minus: return ImGuiKey.Minus;
                case Key.Period: return ImGuiKey.Period;
                case Key.OEM2: return ImGuiKey.Slash;
                case Key.OEM1: return ImGuiKey.Semicolon;
                // FIXME: This is weird... we should do something about the key situation in PAL2.
                case Key.Plus: return ImGuiKey.Equal;
                case Key.OEM4: return ImGuiKey.LeftBracket;
                case Key.OEM5: return ImGuiKey.Backslash;
                case Key.OEM6: return ImGuiKey.RightBracket;
                case Key.OEM3: return ImGuiKey.GraveAccent;
                case Key.CapsLock: return ImGuiKey.CapsLock;
                case Key.ScrollLock: return ImGuiKey.ScrollLock;
                case Key.NumLock: return ImGuiKey.NumLock;
                case Key.PrintScreen: return ImGuiKey.PrintScreen;
                case Key.PauseBreak: return ImGuiKey.Pause;
                case Key.KeypadDecimal: return ImGuiKey.KeypadDecimal;
                case Key.KeypadDivide: return ImGuiKey.KeypadDivide;
                case Key.KeypadMultiply: return ImGuiKey.KeypadMultiply;
                case Key.KeypadSubtract: return ImGuiKey.KeypadSubtract;
                case Key.KeypadAdd: return ImGuiKey.KeypadAdd;
                case Key.KeypadEnter: return ImGuiKey.KeypadEnter;
                case Key.KeypadEqual: return ImGuiKey.KeypadEqual;
                case Key.LeftShift: return ImGuiKey.LeftShift;
                case Key.LeftControl: return ImGuiKey.LeftCtrl;
                case Key.LeftAlt: return ImGuiKey.LeftAlt;
                case Key.LeftGUI: return ImGuiKey.LeftSuper;
                case Key.RightShift: return ImGuiKey.RightShift;
                case Key.RightControl: return ImGuiKey.RightCtrl;
                case Key.RightAlt: return ImGuiKey.RightAlt;
                case Key.RightGUI: return ImGuiKey.RightSuper;
                case Key.Application: return ImGuiKey.Menu;
                default: return ImGuiKey.None;
            }
        }

        static void UpdateKeyModifiers(KeyModifier mods)
        {
            var io = ImGui.GetIO();
            io.AddKeyEvent(ImGuiKey.ModCtrl, mods.HasFlag(KeyModifier.Control));
            io.AddKeyEvent(ImGuiKey.ModShift, mods.HasFlag(KeyModifier.Shift));
            io.AddKeyEvent(ImGuiKey.ModAlt, mods.HasFlag(KeyModifier.Alt));
            io.AddKeyEvent(ImGuiKey.ModSuper, mods.HasFlag(KeyModifier.GUI));
        }

        // FIXME: Once we have Toolkit.Window.GetOpenGLContext we don't need to pass the OpenGL context here...
        public static bool Init(WindowHandle window, OpenGLContextHandle glContext)
        {
            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
            io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
            io.BackendFlags |= ImGuiBackendFlags.HasMouseHoveredViewport;

            BackendData* bd = (BackendData*)NativeMemory.AllocZeroed((uint)sizeof(BackendData));
            io.BackendPlatformUserData = (nint)bd;
            io.NativePtr->BackendPlatformName = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("opentk_impl_opentk_pal2"u8));

            bd->Context = ImGui.GetCurrentContext();
            bd->WindowID = RegisterWindow(window, glContext);
            bd->WantUpdateMonitors = true;

            var platformIO = ImGui.GetPlatformIO();
            platformIO.NativePtr->Platform_SetClipboardTextFn = (nint)(delegate* unmanaged[Cdecl]<nint, byte*, void>)(&Platform_SetClipboardText);
            platformIO.NativePtr->Platform_GetClipboardTextFn = (nint)(delegate* unmanaged[Cdecl]<nint, byte*>)(&Platform_GetClipboardText);

            platformIO.NativePtr->Monitors = default;

            UpdateMonitors();

            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
            mainViewport.PlatformHandle = bd->WindowID;

            InitMultiViewportSupport();

            EventQueue.EventRaised += EventQueue_EventRaised;

            return true;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetClipboardText(nint ctx, byte* text)
        {
            Toolkit.Clipboard.SetClipboardText(Marshal.PtrToStringUTF8((nint)text));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte* Platform_GetClipboardText(nint ctx)
        {
            BackendData* bd = GetBackendData();
            string text = Toolkit.Clipboard.GetClipboardText();
            if (bd->NativeClipboardText != 0)
                Marshal.FreeCoTaskMem(bd->NativeClipboardText);
            bd->NativeClipboardText = Marshal.StringToCoTaskMemUTF8(text);
            return (byte*)bd->NativeClipboardText;
        }

        public static void Shutdown()
        {
            BackendData* bd = GetBackendData();
            var io = ImGui.GetIO();

            EventQueue.EventRaised -= EventQueue_EventRaised;

            ShutdownMultiViewportSupport();

            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();

            if (bd->NativeClipboardText != 0)
            {
                Marshal.FreeCoTaskMem(bd->NativeClipboardText);
            }

            io.NativePtr->BackendPlatformName = null;
            io.BackendPlatformUserData = 0;
            io.BackendFlags &= ~(ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos | ImGuiBackendFlags.HasGamepad);

            NativeMemory.Free(bd);
        }

        static void UpdateMouseData(WindowHandle window)
        {
            var io = ImGui.GetIO();
            BackendData* bd = GetBackendData();

            bool isFocused = Toolkit.Window.IsFocused(window);
            if (isFocused)
            {
                if (io.WantSetMousePos)
                {
                    Toolkit.Mouse.SetGlobalPosition((io.MousePos.X, io.MousePos.Y));
                }

                // FIXME: Mouse passthrough...?
            }

            if (io.BackendFlags.HasFlag(ImGuiBackendFlags.HasMouseHoveredViewport))
            {
                ImGuiViewportPtr viewport = ImGui.FindViewportByPlatformHandle(bd->MouseWindowID);
                uint viewportID = viewport.NativePtr == null ? 0 : viewport.ID;
                io.AddMouseViewportEvent(viewportID);
                if (viewportID != lastViewportID)
                {
                    Console.WriteLine($"Current viewport: {viewportID} (frame: {ImGui.GetFrameCount()})");
                    lastViewportID = viewportID;
                }
                
            }
        }
        static uint lastViewportID = 0;

        static void UpdateMouseCursor(WindowHandle window)
        {
            var io = ImGui.GetIO();
            BackendData* bd = GetBackendData();
            if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
                return;

            ImGuiMouseCursor imguiCursor = ImGui.GetMouseCursor();
            if (io.MouseDrawCursor || imguiCursor == ImGuiMouseCursor.None)
            {
                Toolkit.Window.SetCursor(window, null);
            }
            else
            {
                SystemCursorType cursorType = GetCursor(imguiCursor);
                if (bd->CurrentCursorType != cursorType)
                {
                    bd->CurrentCursorType = cursorType;
                    CursorHandle cursor = Toolkit.Cursor.Create(cursorType);
                    Toolkit.Window.SetCursor(window, cursor);
                }
            }

            static SystemCursorType GetCursor(ImGuiMouseCursor imguiCursor)
            {
                switch (imguiCursor)
                {
                    case ImGuiMouseCursor.Arrow:
                        return SystemCursorType.Default;
                    case ImGuiMouseCursor.TextInput:
                        return SystemCursorType.TextBeam;
                    case ImGuiMouseCursor.ResizeAll:
                        return SystemCursorType.ArrowFourway;
                    case ImGuiMouseCursor.ResizeNS:
                        return SystemCursorType.ArrowNS;
                    case ImGuiMouseCursor.ResizeEW:
                        return SystemCursorType.ArrowEW;
                    case ImGuiMouseCursor.ResizeNESW:
                        return SystemCursorType.ArrowNESW;
                    case ImGuiMouseCursor.ResizeNWSE:
                        return SystemCursorType.ArrowNWSE;
                    case ImGuiMouseCursor.Hand:
                        return SystemCursorType.Hand;
                    case ImGuiMouseCursor.NotAllowed:
                        return SystemCursorType.Forbidden;
                    case ImGuiMouseCursor.None:
                    default:
                        return SystemCursorType.Default;
                }
            }
        }


        static void UpdateMonitors()
        {
            var io = ImGui.GetIO();
            var platformIO = ImGui.GetPlatformIO();
            BackendData* bd = GetBackendData();

            bd->WantUpdateMonitors = false;

            int displayCount = Toolkit.Display.GetDisplayCount();
            if (displayCount == 0)
                return;

            if (platformIO.NativePtr->Monitors.Data != 0)
                Marshal.FreeHGlobal(platformIO.NativePtr->Monitors.Data);
            platformIO.NativePtr->Monitors = new ImVector(displayCount, displayCount, Marshal.AllocHGlobal(displayCount * sizeof(ImGuiPlatformMonitor)));

            NativeMemory.Clear((void*)platformIO.NativePtr->Monitors.Data, (nuint)(platformIO.NativePtr->Monitors.Capacity * sizeof(ImGuiPlatformMonitor)));
            for (int i = 0; i < displayCount; i++)
            {
                ref ImGuiPlatformMonitor imguiMonitor = ref Unsafe.Add(ref Unsafe.AsRef<ImGuiPlatformMonitor>((void*)platformIO.Monitors.Data), i);

                DisplayHandle displayHandle = Toolkit.Display.Open(i);
                Toolkit.Display.GetVirtualPosition(displayHandle, out int posX, out int posY);
                Toolkit.Display.GetResolution(displayHandle, out int resX, out int resY);
                Toolkit.Display.GetWorkArea(displayHandle, out Box2i workArea);
                Toolkit.Display.GetDisplayScale(displayHandle, out float scaleX, out float scaleY);
                
                imguiMonitor.MainPos = new(posX, posY);
                imguiMonitor.MainSize = new(resX, resY);

                imguiMonitor.WorkPos = new(workArea.Min.X, workArea.Min.Y);
                imguiMonitor.WorkSize = new(workArea.Size.X, workArea.Size.Y);
                imguiMonitor.DpiScale = scaleX;
                imguiMonitor.PlatformHandle = (void*)i;
            }
        }


        public static void NewFrame()
        {
            var io = ImGui.GetIO();
            BackendData* bd = GetBackendData();
            if (TryGetWindowFromID(bd->WindowID, out WindowHandle window) == false)
            {
                throw new InvalidOperationException("Could not find main window...");
            }

            Toolkit.Window.GetFramebufferSize(window, out Vector2i fbSize);
            io.DisplaySize = new(fbSize.X, fbSize.Y);
            Toolkit.Window.GetScaleFactor(window, out float scaleX, out float scaleY);
            io.DisplayFramebufferScale = new(scaleX, scaleY);

            if (bd->WantUpdateMonitors)
            {
                UpdateMonitors();
            }

            var currentTime = Stopwatch.GetTimestamp();
            if (currentTime <= bd->Time)
            {
                currentTime = bd->Time + 1;
            }
            io.DeltaTime = bd->Time > 0.0 ? (currentTime - bd->Time) / (float)Stopwatch.Frequency : 1.0f / 60.0f;
            bd->Time = currentTime;

            Toolkit.Mouse.GetGlobalMouseState(out MouseState mouseState);
            if (bd->MousePendingLeaveFrame != 0 && bd->MousePendingLeaveFrame >= ImGui.GetFrameCount() && mouseState.PressedButtons == 0)
            {
                bd->MouseWindowID = 0;
                bd->MousePendingLeaveFrame = 0;
                io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
                Console.WriteLine($"Resetting mouse window. (frame: {ImGui.GetFrameCount()})");
            }

            UpdateMouseData(window);
            UpdateMouseCursor(window);

            // Update game controllers (if enabled and available)
            //UpdateGamepads();
        }

        private static void EventQueue_EventRaised(PalHandle handle, PlatformEventType type, EventArgs args)
        {
            var io = ImGui.GetIO();
            BackendData* bd = GetBackendData();

            if (args is WindowEventArgs windowEvent)
            {
                if (TryGetIDFromWindow(windowEvent.Window, out _) == false)
                {
                    // Any window we don't know about, ignore it.
                    return;
                }
            }

            if (args is MouseMoveEventArgs mouseMove)
            {
                float x = mouseMove.ClientPosition.X;
                float y = mouseMove.ClientPosition.Y;

                if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
                {
                    Toolkit.Window.ClientToScreen(mouseMove.Window, (x, y), out Vector2 screenPos);

                    x = screenPos.X;
                    y = screenPos.Y;
                }

                io.AddMousePosEvent(x, y);
            }
            else if (args is ScrollEventArgs scroll)
            {
                io.AddMouseWheelEvent(scroll.Delta.X, scroll.Delta.Y);
            }
            else if (args is MouseButtonDownEventArgs mouseDown)
            {
                if (mouseDown.Button >= 0 && mouseDown.Button < (MouseButton)ImGuiMouseButton.COUNT)
                {
                    io.AddMouseButtonEvent((int)mouseDown.Button, true);
                }

                Console.WriteLine($"Mouse btn down {mouseDown.Button}.");
            }
            else if (args is MouseButtonUpEventArgs mouseUp)
            {
                if (mouseUp.Button >= 0 && mouseUp.Button < (MouseButton)ImGuiMouseButton.COUNT)
                {
                    io.AddMouseButtonEvent((int)mouseUp.Button, false);
                }
                Console.WriteLine($"Mouse btn up {mouseUp.Button}.");
            }
            else if (args is TextInputEventArgs text)
            {
                nint utf8 = Marshal.StringToCoTaskMemUTF8(text.Text);
                ImGuiNative.ImGuiIO_AddInputCharactersUTF8(io.NativePtr, (byte*)utf8);
                Marshal.FreeCoTaskMem(utf8);
            }
            else if (args is KeyDownEventArgs keyDown)
            {
                UpdateKeyModifiers(keyDown.Modifiers);
                ImGuiKey imguiKey = TranslateKey(keyDown.Key);
                io.AddKeyEvent(imguiKey, true);
            }
            else if (args is KeyUpEventArgs keyUp)
            {
                UpdateKeyModifiers(keyUp.Modifiers);
                ImGuiKey imguiKey = TranslateKey(keyUp.Key);
                io.AddKeyEvent(imguiKey, false);
            }
            else if (args is DisplayConnectionChangedEventArgs displayEvent)
            {
                bd->WantUpdateMonitors = true;
            }
            else if (args is MouseEnterEventArgs mouseEnter)
            {
                if (mouseEnter.Entered)
                {
                    if (TryGetIDFromWindow(mouseEnter.Window, out nint windowID) == false)
                        throw new UnreachableException("We should already have filtered out all windows we don't know about.");
                    bd->MouseWindowID = windowID;
                    bd->MousePendingLeaveFrame = 0;
                }
                else
                {
                    // FIXME: Something about the pending frames...?
                    //bd->MousePendingLeaveFrame = ImGui.GetFrameCount() + 1;
                }

                string title = Toolkit.Window.GetTitle(mouseEnter.Window);
                Console.WriteLine($"Mouse {(mouseEnter.Entered ? "entered" : "left")} window '{title}'. (frame: {ImGui.GetFrameCount()})");
            }
            else if (args is FocusEventArgs focus)
            {
                io.AddFocusEvent(focus.GotFocus);
                string title = Toolkit.Window.GetTitle(focus.Window);
                Console.WriteLine($"Window '{title}' {(focus.GotFocus ? "got focus." : "lost focus.")} (frame: {ImGui.GetFrameCount()})");
            }
            else if (args is WindowMoveEventArgs windowMove)
            {
                if (TryGetIDFromWindow(windowMove.Window, out nint windowID) == false)
                    throw new UnreachableException("We should already have filtered out all windows we don't know about.");
                ImGuiViewportPtr viewport = ImGui.FindViewportByPlatformHandle(windowID);
                viewport.PlatformRequestMove = true;
            }
            else if (args is WindowResizeEventArgs windowResize)
            {
                if (TryGetIDFromWindow(windowResize.Window, out nint windowID) == false)
                    throw new UnreachableException("We should already have filtered out all windows we don't know about.");
                ImGuiViewportPtr viewport = ImGui.FindViewportByPlatformHandle(windowID);
                viewport.PlatformRequestResize = true;
            }
            else if (args is CloseEventArgs windowClose)
            {
                if (TryGetIDFromWindow(windowClose.Window, out nint windowID) == false)
                    throw new UnreachableException("We should already have filtered out all windows we don't know about.");
                ImGuiViewportPtr viewport = ImGui.FindViewportByPlatformHandle(windowID);
                viewport.PlatformRequestClose = true;
            }
        }

        struct ViewportData
        {
            public nint WindowID;
            public bool WindowOwned;
            public int IgnoreWindowPosEventFrame;
            public int IgnoreWindowSizeEventFrame;
        }

        static void InitMultiViewportSupport()
        {
            var platformIO = ImGui.GetPlatformIO();
            BackendData* bd = GetBackendData();

            platformIO.Platform_CreateWindow = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_CreateWindow;
            platformIO.Platform_DestroyWindow = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_DestroyWindow;
            platformIO.Platform_ShowWindow = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_ShowWindow;
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowPos(platformIO, (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2*, void>)&Platform_GetWindowPos);
            platformIO.Platform_SetWindowPos = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2, void>)&Platform_SetWindowPos;
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowSize(platformIO, (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2*, void>)&Platform_GetWindowSize);
            platformIO.Platform_SetWindowSize = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, SNVector2, void>)&Platform_SetWindowSize;
            platformIO.Platform_SetWindowTitle = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, nint, void>)&Platform_SetWindowTitle;
            platformIO.Platform_SetWindowFocus = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Platform_SetWindowFocus;
            platformIO.Platform_GetWindowFocus = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, byte>)&Platform_GetWindowFocus;
            platformIO.Platform_GetWindowMinimized = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, byte>)&Platform_GetWindowMinimized;
            platformIO.Platform_SetWindowAlpha = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, float, void>)&Platform_SetWindowAlpha;
            platformIO.Platform_RenderWindow = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void*, void>)&Platform_RenderWindow;
            platformIO.Platform_SwapBuffers = (nint)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void*, void>)&Platform_SwapBuffers;

            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
            ViewportData* vd = (ViewportData*)NativeMemory.AllocZeroed((uint)sizeof(ViewportData));
            vd->WindowID = bd->WindowID;
            vd->WindowOwned = false;
            mainViewport.PlatformUserData = (nint)vd;
            mainViewport.PlatformHandle = bd->WindowID;
        }

        static void ShutdownMultiViewportSupport()
        {
            ImGui.DestroyPlatformWindows();
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_CreateWindow(ImGuiViewportPtr viewport)
        {
            BackendData* bd = GetBackendData();
            ViewportData* vd = (ViewportData*)NativeMemory.AllocZeroed((uint)sizeof(ViewportData));
            viewport.PlatformUserData = (nint)vd;

            bool useOpenGL = TryGetContextFromID(ImGui.GetMainViewport().PlatformHandle, out OpenGLContextHandle shareContext);

            GraphicsApiHints graphicsSettings;
            if (useOpenGL)
            {
                graphicsSettings = new OpenGLGraphicsApiHints()
                {
                    SharedContext = shareContext,
                };
            }
            else
            {
                graphicsSettings = new VulkanGraphicsApiHints();
            }

            OpenTK.Core.Utility.LogLevel prevFilter = Toolkit.Window.Logger.Filter;
            Toolkit.Window.Logger.Filter = OpenTK.Core.Utility.LogLevel.Info;
            WindowHandle window = Toolkit.Window.Create(graphicsSettings);
            Toolkit.Window.Logger.Filter = prevFilter;

            if (viewport.Flags.HasFlag(ImGuiViewportFlags.NoDecoration))
            {
                Toolkit.Window.SetBorderStyle(window, WindowBorderStyle.Borderless);
            }
            else
            {
                Toolkit.Window.SetBorderStyle(window, WindowBorderStyle.ResizableBorder);
            }

            if (viewport.Flags.HasFlag(ImGuiViewportFlags.NoTaskBarIcon))
            {
                Toolkit.Window.SetBorderStyle(window, WindowBorderStyle.ToolBox);
            }

            if (viewport.Flags.HasFlag(ImGuiViewportFlags.TopMost))
            {
                Toolkit.Window.SetAlwaysOnTop(window, true);
            }

            Toolkit.Window.SetTitle(window, "No title yet.");
            Toolkit.Window.SetClientPosition(window, ((int)viewport.Pos.X, (int)viewport.Pos.Y));
            Toolkit.Window.SetClientSize(window, ((int)viewport.Size.X, (int)viewport.Size.Y));

            OpenGLContextHandle previousContext = Toolkit.OpenGL.GetCurrentContext();

            OpenGLContextHandle glContext = null;
            if (useOpenGL)
            {
                Toolkit.Window.Logger.Filter = OpenTK.Core.Utility.LogLevel.Info;
                glContext = Toolkit.OpenGL.CreateFromWindow(window);
                Toolkit.Window.Logger.Filter = prevFilter;

                Toolkit.OpenGL.SetCurrentContext(glContext);
                Toolkit.OpenGL.SetSwapInterval(0);

                Toolkit.OpenGL.SetCurrentContext(previousContext);
            }

            nint windowID = RegisterWindow(window, glContext);

            vd->WindowID = windowID;
            vd->WindowOwned = true;
            viewport.PlatformHandle = windowID;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_DestroyWindow(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            if (vd != null)
            {
                if (vd->WindowOwned)
                {
                    OpenGLContextHandle glContext = GetContextFromID(vd->WindowID);
                    if (glContext != null)
                    {
                        Toolkit.OpenGL.DestroyContext(glContext);
                    }

                    WindowHandle window = GetWindowFromID(vd->WindowID);
                    if (window != null)
                    {
                        Toolkit.Window.Destroy(window);
                    }
                }

                NativeMemory.Free(vd);
            }
            viewport.PlatformUserData = 0;
            viewport.PlatformHandle = 0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_ShowWindow(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            if (viewport.Flags.HasFlag(ImGuiViewportFlags.NoFocusOnAppearing))
            {
                // FIXME: ??
            }

            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.SetMode(window, WindowMode.Normal);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_GetWindowPos(ImGuiViewportPtr viewport, SNVector2* outPos)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.GetClientPosition(window, out Vector2i clientPosition);
            *outPos = new(clientPosition.X, clientPosition.Y);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowPos(ImGuiViewportPtr viewport, SNVector2 pos)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.SetClientPosition(window, ((int)pos.X, (int)pos.Y));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_GetWindowSize(ImGuiViewportPtr viewport, SNVector2* outSize)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.GetClientSize(window, out Vector2i clientSize);
            *outSize = new(clientSize.X, clientSize.Y);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowSize(ImGuiViewportPtr viewport, SNVector2 size)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.SetClientSize(window, ((int)size.X, (int)size.Y));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowTitle(ImGuiViewportPtr viewport, nint name)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.SetTitle(window, Marshal.PtrToStringUTF8(name));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowFocus(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.FocusWindow(window);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte Platform_GetWindowFocus(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            return Toolkit.Window.IsFocused(window) ? (byte)1 : (byte)0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte Platform_GetWindowMinimized(ImGuiViewportPtr viewport)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            return Toolkit.Window.GetMode(window) == WindowMode.Minimized ? (byte)1 : (byte)0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SetWindowAlpha(ImGuiViewportPtr viewport, float alpha)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            WindowHandle window = GetWindowFromID(vd->WindowID);
            Toolkit.Window.SetTransparencyMode(window, WindowTransparencyMode.TransparentWindow, alpha);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_RenderWindow(ImGuiViewportPtr viewport, void* _)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            OpenGLContextHandle glContext = GetContextFromID(vd->WindowID);
            if (glContext != null)
            {
                Toolkit.OpenGL.SetCurrentContext(glContext);
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Platform_SwapBuffers(ImGuiViewportPtr viewport, void* _)
        {
            ViewportData* vd = (ViewportData*)viewport.PlatformUserData;
            OpenGLContextHandle glContext = GetContextFromID(vd->WindowID);
            if (glContext != null)
            {
                Toolkit.OpenGL.SetCurrentContext(glContext);
                Toolkit.OpenGL.SwapBuffers(glContext);
            }
        }
    }
}
