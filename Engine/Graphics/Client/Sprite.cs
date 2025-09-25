using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Sprite : EObject
    {
        public Texture2D Texture { get; set; }
        public int AtlasIndex { get; set; }

        public AtlasChunk GetAtlasChunk()
        {
            if (Texture)
            {
                var chunk = Texture.Atlas.GetChunk(AtlasIndex);

                if (chunk.Width == 0 && chunk.Height == 0)
                {
                    chunk.Width = Texture.Width;
                    chunk.Height = Texture.Height;
                }

                return chunk;
            }

#if DEBUG
            Debug.Error($"Sprite: {Name}, doesn't have a texture attached, using default atlas chunk instead.");
#endif
            return AtlasChunk.DefaultChunk;
        }
    }
}
