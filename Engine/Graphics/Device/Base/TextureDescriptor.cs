using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class TextureDescriptor : IResourceDescriptor
    {
        internal int Width { get; set; }
        internal int Height { get; set; }
        internal byte[] Buffer { get; set; }
    }
}
