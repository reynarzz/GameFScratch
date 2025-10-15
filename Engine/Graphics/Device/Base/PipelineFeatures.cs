using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal class PipelineFeatures
    {
        internal bool DepthBuffer { get; set; } = false;
        internal Blending Blending { get; set; } = new();
        internal Stencil Stencil { get; set; } = new();
    }
}
