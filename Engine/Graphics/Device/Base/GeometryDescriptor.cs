using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class VertexAtrib
    {
        public int Count { get; set; }
        public GfxValueType Type { get; set; }
        public bool Normalized { get; set; }
        public int Stride { get; set; }
        public int Offset { get; set; }
    }

    internal class VertexDataDescriptor
    {
        public BufferDataDescriptor BufferDesc { get; set; }
        public List<VertexAtrib> Attribs { get; set; }
    }

    internal class GeometryDescriptor : IResourceDescriptor
    {
        

        public VertexDataDescriptor VertexDesc { get; set; }
        public BufferDataDescriptor IndexBuffer { get; set; }
    }
}
