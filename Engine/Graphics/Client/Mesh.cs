using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct QuadVertices
    {
        public Vertex v0 { get; set; }
        public Vertex v1 { get; set; }
        public Vertex v2 { get; set; }
        public Vertex v3 { get; set; }
    }

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
        internal bool IsDirty { get; private set; }
        public List<Vertex> Vertices { get; }
        public List<uint> Indices { get; }
        public int IndicesToDrawCount { get; set; }

        public Mesh()
        {
            Vertices = new List<Vertex>();
            Indices = new List<uint>();
        }
    }
}
