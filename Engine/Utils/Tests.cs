using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utils
{
    internal class Tests
    {
        private static string Vertex = @"
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

        private static string fragment = @"
       #version 330 core

        out vec4 color;

        void main()
        {
            color = vec4(1.0, 1.0, 1.0, 1.0); 
        }";

        private static string fragmentTex = @"
           #version 330 core
            in vec2 fragUV;
            out vec4 color;

            uniform sampler2D uTexture; // uniform for the texture

            void main()
            {
                color = texture(uTexture, fragUV);
            }";



        internal static ShaderDescriptor GetTestShaderDescriptor()
        {
            var shaderDescriptor = new ShaderDescriptor();
            shaderDescriptor.VertexSource = Encoding.UTF8.GetBytes(Vertex);
            shaderDescriptor.FragmentSource = Encoding.UTF8.GetBytes(fragmentTex);
            return shaderDescriptor;
        }

        internal static unsafe GeometryDescriptor GetTestGeometryDescriptor()
        {
            var indices = new uint[6]
            {
               0, 1, 2,
               0, 2, 3
            };


            var vertices = new float[]
            {
                // x,     y,    z,    u,   v
                -0.5f, -0.5f, 0.0f,  0.0f, 0.0f,  // bottom-left
                -0.5f,  0.5f, 0.0f,  0.0f, 1.0f,   // top-left
                 0.5f,  0.5f, 0.0f,  1.0f, 1.0f,  // top-right
                 0.5f, -0.5f, 0.0f,  1.0f, 0.0f,  // bottom-right
            };

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

            return geoDesc;
        }

        internal static TextureDescriptor TestTextureCreation()
        {
            var textureDescriptor = new TextureDescriptor();
            textureDescriptor.Width = 256;
            textureDescriptor.Height = 256;

            var bufferSize = textureDescriptor.Width * textureDescriptor.Height * 4;
            textureDescriptor.Buffer = new byte[bufferSize];

            new Random().NextBytes(textureDescriptor.Buffer);

            return textureDescriptor;
        }
    }
}
