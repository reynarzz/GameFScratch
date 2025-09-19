using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFW;
using OpenGL;

namespace Engine
{
    internal class Window
    {
        public  bool IsFullScreen { get; private set; }
        private GLFW.Window _glfwWindow;
        private int _width = 920;
        private int _height = 600;

        private string _windowName = "Game";
        public string Name 
        {
            get => _windowName; 
            set 
            { 
                _windowName = value; 
                GLFW.Glfw.SetWindowTitle(_glfwWindow, _windowName); 
            }
        }

        public Window(string name, int width, int height)
        {
            _windowName = name;
            _width = width;
            _height = height;

            Glfw.Init();

            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            Glfw.WindowHint(Hint.ContextVersionMinor, 2);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.Resizable, false);


            // Create a window
            _glfwWindow = Glfw.CreateWindow(_width, _height, name, default, default);

            
            if (_glfwWindow == GLFW.Window.None)
            {
                Console.WriteLine("Failed to create GLFW window");
                Glfw.Terminate();
                return;
            }
            Glfw.MakeContextCurrent(_glfwWindow);

            GL.Import(Glfw.GetProcAddress);

            Glfw.SwapInterval(1);
            
            while (!Glfw.WindowShouldClose(_glfwWindow))
            {
                Glfw.PollEvents();

                GL.glClearColor(1, 0, 0, 1);
                GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);


                Glfw.SwapBuffers(_glfwWindow);
            }
        }

        public void FullScreen(bool fullscreen)
        {
            if (IsFullScreen)
            {
                // Get primary monitor and video mode
                GLFW.Monitor monitor = Glfw.Monitors[0];
                var mode = Glfw.GetVideoMode(monitor);

                // Switch to fullscreen
                Glfw.SetWindowMonitor(
                    _glfwWindow,
                    monitor,
                    0, 0,
                    mode.Width,
                    mode.Height,
                    mode.RefreshRate
                );
            }
            else
            {
                // Switch back to windowed mode
                Glfw.SetWindowMonitor(
                    _glfwWindow,
                    GLFW.Monitor.None,
                    100,
                    100,
                    _width,
                    _height,
                    0
                );
            }
        }
    }
}
