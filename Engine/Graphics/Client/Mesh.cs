using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex 
    {
        public vec3 Position;
        public vec2 UV;
        public vec3 Normals;
        public ColorPacketRGBA Color;
        public int TextureIndex;
    }

    public class Mesh : EObject
    {
        public Vertex[] Vertices { get; set; }
        public uint[] Indices { get; set; }
    }
}
