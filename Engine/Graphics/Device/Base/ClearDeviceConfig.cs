using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal struct ClearDeviceConfig
    {
        public Color Color { get; set; }
        public GfxResource RenderTarget { get; set; }

    }
}
