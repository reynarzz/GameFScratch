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

    internal abstract class BufferDataDescriptor : IGfxResourceDescriptor
    {
        internal int Count { get; set; }
        internal int Offset { get; set; }

        internal abstract int BufferLength { get; }
        internal BufferUsage Usage { get; set; }

        internal abstract IntPtr GetBufferUnsafePtr();      
    }

    internal unsafe class BufferDataDescriptor<T> : BufferDataDescriptor
    {
        internal T[] Buffer { get; set; }
        internal override int BufferLength => Buffer.Length * sizeof(T);

        internal override IntPtr GetBufferUnsafePtr()
        {
            fixed (void* ptr = Buffer)
            {
                return new nint(ptr);
            }
        }
    }
}
