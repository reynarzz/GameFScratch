using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct Vertex 
    {
        public vec3 Position { get; set; }
        public vec2 UV { get; set; }
        public uint Color { get; set; }
        public int TextureIndex { get; set; }
    }

    public class Mesh : EObject
    {
        public Vertex[] Vertices { get; set; }
        public uint[] Indices { get; set; }
    }
}
