using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Graphics.OpenGL;
using GLFW;
using OpenGL;

namespace Engine
{
    public class Window
    {
        public static bool IsFullScreen { get; private set; }
        public static int Width { get; private set; } = 920;
        public static int Height { get; private set; } = 600;

        private static int _startWidth;
        private static int _startHeight;
        private static string _windowName = "Game";
        public static event Action<int, int> OnWindowChanged;

        private static bool _isMouseVisible = true;
        public static bool MouseVisible
        {
            get => _isMouseVisible;

            set
            {
                _isMouseVisible = value;

                if (_isMouseVisible)
                {
                    Glfw.SetInputMode(NativeWindow, InputMode.Cursor, (int)CursorMode.Normal);
                }
                else
                {
                    Glfw.SetInputMode(NativeWindow, InputMode.Cursor, (int)CursorMode.Disabled);
                }
            }
        }
        public static string Name
        {
            get => _windowName;
            set
            {
                if (_windowName == value)
                {
                    return;
                }

                _windowName = value;
                GLFW.Glfw.SetWindowTitle(NativeWindow, _windowName);
            }
        }

        internal static bool ShouldClose => Glfw.WindowShouldClose(NativeWindow);

        public int MonitorCount => Glfw.Monitors.Length;

        internal static GLFW.Window NativeWindow { get; private set; }

        internal Window(string name, int width, int height)
        {
            _windowName = name;
            Width = width;
            Height = height;
            _startWidth = width;
            _startHeight = height;

            Glfw.Init();

            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 2);
            Glfw.WindowHint(Hint.Resizable, false);


            // Create a window
            NativeWindow = Glfw.CreateWindow(Width, Height, name, default, default);

            if (NativeWindow == GLFW.Window.None)
            {
                Console.WriteLine("Failed to create GLFW window");
                Glfw.Terminate();
                return;
            }
            Glfw.MakeContextCurrent(NativeWindow);

            GL.Import(Glfw.GetProcAddress);

            Glfw.SwapInterval(1);
        }

        internal static void SwapBuffers()
        {
            Glfw.SwapBuffers(NativeWindow);
        }

        public static void SetWindowSize(int width, int height)
        {
            var mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);

            Width = Math.Clamp(width, 1, mode.Width);
            Height = Math.Clamp(height, 1, mode.Height);

            Glfw.SetWindowSize(NativeWindow, Width, Height);

            OnWindowChanged?.Invoke(Width, Height);
        }

        public static void SetWindowPosition(int x, int y)
        {
            Glfw.SetWindowPosition(NativeWindow, x, y);
        }

        public static void FullScreen(bool fullscreen, int monitorIndex = 0)
        {
            IsFullScreen = fullscreen;

            if (fullscreen)
            {
                if (Glfw.Monitors.Length <= monitorIndex)
                {
                    Debug.Error($"Monitor index '{monitorIndex}' is bigger than physical monitors '{Glfw.Monitors.Length}'.");
                    return;
                }

                // Get primary monitor and video mode
                GLFW.Monitor monitor = Glfw.Monitors[monitorIndex];
                var mode = Glfw.GetVideoMode(monitor);

                Width = mode.Width;
                Height = mode.Height;

                OnWindowChanged?.Invoke(Width, Height);

                // Switch to fullscreen
                Glfw.SetWindowMonitor(
                    NativeWindow,
                    monitor,
                    0, 0,
                    mode.Width,
                    mode.Height,
                    mode.RefreshRate
                );

            }
            else
            {
                Width = _startWidth;
                Height = _startHeight;
                OnWindowChanged?.Invoke(_startWidth, _startHeight);

                // Switch back to windowed mode
                Glfw.SetWindowMonitor(
                    NativeWindow,
                    GLFW.Monitor.None,
                    100,
                    100,
                    Width,
                    Height,
                    0
                );


            }
        }
    }
}
