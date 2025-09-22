using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Rendering
{
    internal class Batch
    {
        internal Material Material { get; set; }
        internal GfxResource Geometry { get; set; }
        
        internal int VertexCount { get; private set; }
        internal int IndexCount { get; private set; }
        public Dictionary<Texture, uint> Textures { get; set; }
    }
}
