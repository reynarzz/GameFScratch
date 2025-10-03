using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class RenderTargetDescriptor : IGfxResourceDescriptor
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
