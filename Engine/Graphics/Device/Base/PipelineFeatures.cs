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
        internal Stencil Stencil { get; set; }
    }

    public struct Blending 
    {
        public bool Enabled { get; set; }
    }

    public struct Stencil
    {
        public bool Enabled { get; set; }
    }
}
