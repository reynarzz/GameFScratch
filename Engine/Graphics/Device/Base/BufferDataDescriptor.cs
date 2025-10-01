using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal enum BufferUsage
    {
        Invalid,
        Static,
        Dynamic,
        Stream,
    }

    internal unsafe class BufferDataDescriptor : ResourceDescriptorBase
    {
        internal int Count { get; set; }
        internal int Offset { get; set; }

        internal byte[] Buffer { get; set; }

        internal BufferUsage Usage { get; set; }
    }
}
