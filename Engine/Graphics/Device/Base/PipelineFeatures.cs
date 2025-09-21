using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class PipelineFeatures
    {
        internal bool DepthBuffer { get; set; } = false;
        internal Blending Blending { get; set; }
        internal bool StencilBuffer { get; set; } = false;
    }

    internal struct Blending 
    {
        internal bool Enabled { get; set; }
    }
}
