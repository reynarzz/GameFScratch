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

        private static string _windowName = "Game";
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

            TestShaders();
            TestGeometryCreation();
            TestTextureCreation();

        }

        internal void PollEvents()
        {
            Glfw.PollEvents();
        }

        internal void SwapBuffers()
        {
            Glfw.SwapBuffers(NativeWindow);
        }

        private string Vertex = @"
          #version 330 core

        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 uv;

        out vec2 fragUV;

        void main() 
        {
            fragUV = uv;
            gl_Position = vec4(position, 1.0);
        }
        ";

        private string fragment = @"
       #version 330 core

        out vec4 color;

        void main()
        {
            color = vec4(1.0, 1.0, 1.0, 1.0); 
        }";

        private string fragmentTex = @"
           #version 330 core
            in vec2 fragUV;
            out vec4 color;

            uniform sampler2D uTexture; // uniform for the texture

            void main()
            {
                color = texture(uTexture, fragUV);
            }";

        GLShader shader = new GLShader();

        GLGeometry geometry = new GLGeometry();
        GLTexture texture = new GLTexture();

        uint[] indices = new uint[6]
        {
            0, 1, 2,
            0, 2, 3
        };


        float[] vertices = new float[]
        {
                // x,     y,    z,    u,   v
                -0.5f, -0.5f, 0.0f,  0.0f, 0.0f,  // bottom-left
                -0.5f,  0.5f, 0.0f,  0.0f, 1.0f,   // top-left
                 0.5f,  0.5f, 0.0f,  1.0f, 1.0f,  // top-right
                 0.5f, -0.5f, 0.0f,  1.0f, 0.0f,  // bottom-right
        };


        private void TestShaders()
        {
            var shaderDescriptor = new ShaderDescriptor();
            shaderDescriptor.VertexSource = Encoding.UTF8.GetBytes(Vertex);
            shaderDescriptor.FragmentSource = Encoding.UTF8.GetBytes(fragmentTex);

            shader.Create(shaderDescriptor);
        }

        private unsafe void TestGeometryCreation()
        {
            var geoDesc = new GeometryDescriptor();

            var vertexDesc = new GeometryDescriptor.VertexDataDescriptor();
            vertexDesc.BufferDesc = new BufferDataDescriptor();

            vertexDesc.BufferDesc.Buffer = System.Runtime.InteropServices.MemoryMarshal.AsBytes<float>(vertices).ToArray();
            vertexDesc.BufferDesc.Usage = BufferUsage.Static;
            vertexDesc.Attribs = new()
            {
                new() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(float) * 5, Offset = 0 },
                new() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(float) * 5, Offset = sizeof(float) * 3 },
            };

            var indexDesc = new BufferDataDescriptor();
            indexDesc.Usage = BufferUsage.Static;
            indexDesc.Buffer = System.Runtime.InteropServices.MemoryMarshal.AsBytes<uint>(indices).ToArray();

            geoDesc.IndexBuffer = indexDesc;
            geoDesc.VertexDesc = vertexDesc;

            geometry.Create(geoDesc);
        }

        private void TestTextureCreation()
        {
            var textureDescriptor = new TextureDescriptor();
            textureDescriptor.Width = 1;
            textureDescriptor.Height = 1;
            textureDescriptor.Buffer = new byte[] { 0x3F, 0x0F, 0xF0, 0xFF };
            texture.Create(textureDescriptor);
        }

        public static void SetWindowSize(int width, int height)
        {
            var mode = Glfw.GetVideoMode(Glfw.Monitors[0]);

            Width = Math.Clamp(width, 1, mode.Width);
            Height = Math.Clamp(height, 1, mode.Height);

            Glfw.SetWindowSize(NativeWindow, Width, Height);
        }

        public static void SetWindowPosition(int x, int y)
        {
            Glfw.SetWindowPosition(NativeWindow, x, y);
        }

        public static void FullScreen(bool fullscreen, int monitorIndex = 0)
        {
            if (IsFullScreen == fullscreen)
                return;

            if (fullscreen)
            {
                if (Glfw.Monitors.Length <= monitorIndex)
                {
                    Log.Error($"Monitor index '{monitorIndex}' is bigger than physical monitors '{Glfw.Monitors.Length}'.");
                    return;
                }

                // Get primary monitor and video mode
                GLFW.Monitor monitor = Glfw.Monitors[monitorIndex];
                var mode = Glfw.GetVideoMode(monitor);

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
