using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal struct GfxDeviceInfo
    {
        internal int MaxTexAccessInVertexShader { get; set; }
        internal int MaxHardwareTextureUnits { get; set; }
        internal int MaxValidTextureUnits { get; set; }
        internal string Vendor { get; set; }
        internal string Renderer { get; set; }
        internal string Version { get; set; }
        internal string DeviceName { get; set; }
    }
}
