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
    internal class Window
    {
        public bool IsFullScreen { get; private set; }
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

            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 2);
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

            TestShaders();
            TestGeometryCreation();
            TestTextureCreation();

            while (!Glfw.WindowShouldClose(_glfwWindow))
            {
                Glfw.PollEvents();

                GL.glClearColor(1, 0, 0, 1);
                GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

                shader.Bind();
                geometry.Bind();
                texture.Bind();

                unsafe
                {
                    GL.glDrawElements(GL.GL_TRIANGLES, indices.Length, GL.GL_UNSIGNED_INT, null);
                }
                Glfw.SwapBuffers(_glfwWindow);
            }
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
            vertexDesc.Attribs = new List<GeometryDescriptor.VertexAtrib>()
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
            textureDescriptor.Buffer = new byte[] { 0x3F, 0xFF, 0xF0, 0xFF };
            texture.Create(textureDescriptor);
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
