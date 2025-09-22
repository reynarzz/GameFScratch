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
            if(Texture)
            {
                if (Texture.Atlas.HasValidChunk(AtlasIndex))
                {
                    return Texture.Atlas.GetChunk(AtlasIndex);
                }
                else
                {
                    var def = AtlasChunk.DefaultChunk;
                    def.Width = Texture.Width;
                    def.Height = Texture.Height;

                    return def;
                }
            }

#if DEBUG
            Log.Error($"Sprite: {Name}, doesn't have a texture attached, using default atlas chunk instead.");
#endif
            return AtlasChunk.DefaultChunk;
        }
    }
}
