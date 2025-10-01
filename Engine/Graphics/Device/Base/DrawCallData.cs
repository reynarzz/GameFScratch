using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal struct IndexedDrawType
    {
        public int IndexCount { get; set; }
    }

    internal struct ArraysDrawType
    {
        public int StartIndex { get; set; }
        public int VertexCount { get; set; }
    }

    internal class DrawCallData
    {
        internal DrawType DrawType { get; set; }
        internal DrawMode DrawMode { get; set; }
        internal PipelineFeatures Features { get; set; }
        internal GfxResource Shader { get; set; }
        internal GfxResource Geometry { get; set; }
        internal GfxResource[] Textures { get; set; }
        internal UniformValue[] Uniforms { get; set; }

        internal IndexedDrawType IndexedDraw;
        internal ArraysDrawType ArraysDraw;
    }

}
