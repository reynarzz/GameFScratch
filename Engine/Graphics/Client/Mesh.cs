using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex 
    {
        public vec3 Position;
        public vec2 UV;
        public vec2 Normals;
        public uint Color;

        // TODO: Will be used in the future when I implement the texture bin packer,
        //public int TextureIndex { get; set; }
    }

    public class Mesh : EObject
    {
        public Vertex[] Vertices { get; set; }
        public uint[] Indices { get; set; }
    }
}
