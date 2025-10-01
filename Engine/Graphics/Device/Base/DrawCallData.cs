using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal struct IndexedDrawType
    {
        public int IndexDrawCount { get; set; }
    }

    internal struct ArraysDrawType
    {
        public int StartIndex { get; set; }
        public int VertexCount { get; set; }
    }

    internal class DrawCallData
    {
        public DrawType DrawType { get; set; }
        public IndexedDrawType IndexedDrawType;
        public ArraysDrawType ArraysDrawType;
        public DrawMode DrawMode { get; set; }
        public PipelineFeatures Features { get; set; }
        public GfxResource Shader { get; set; }
        public GfxResource Geometry { get; set; }
        public GfxResource[] Textures { get; set; }
        public UniformValue[] Uniforms { get; set; }
    }

}
