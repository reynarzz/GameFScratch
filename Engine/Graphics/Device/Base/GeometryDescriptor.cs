using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class VertexAtrib
    {
        internal int Count { get; set; }
        internal GfxValueType Type { get; set; }
        internal bool Normalized { get; set; }
        internal int Stride { get; set; }
        internal int Offset { get; set; }
    }

    internal class VertexDataDescriptor
    {
        internal BufferDataDescriptor BufferDesc { get; set; }
        internal List<VertexAtrib> Attribs { get; set; }
    }

    internal class GeometryDescriptor : ResourceDescriptorBase
    {
        internal VertexDataDescriptor VertexDesc { get; set; }

        /// <summary>
        /// Define an index buffer to be created
        /// </summary>
        internal BufferDataDescriptor IndexDesc { get; set; }

        /// <summary>
        /// Set an indexBuffer, no index buffer will be created, note: shared index buffers cannot be updated by clients.
        /// </summary>
        internal GfxResource SharedIndexBuffer { get; set; }
    }
}
