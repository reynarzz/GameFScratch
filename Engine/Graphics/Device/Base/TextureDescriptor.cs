using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public enum TextureMode
    {
        Clamp,
        Repeat
    }

    internal class TextureDescriptor : IGfxResourceDescriptor
    {
        internal int Width { get; set; }
        internal int Height { get; set; }
        internal int XOffset { get; set; }
        internal int YOffset { get; set; }
        internal int Channels { get; set; }
        internal byte[] Buffer { get; set; }
        internal TextureMode Mode { get; set; } = TextureMode.Clamp;
    }
}
