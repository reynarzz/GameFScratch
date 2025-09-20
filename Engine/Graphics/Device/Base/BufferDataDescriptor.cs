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

    internal class BufferDataDescriptor : ResourceDescriptorBase
    {
        internal int Size { get; set; }
        internal int Count { get; set; }
        internal int Offset { get; set; }
        internal IntPtr Buffer { get; set; }
        internal BufferUsage Usage { get; set; }
    }
}
