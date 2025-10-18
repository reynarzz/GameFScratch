using Engine.Graphics;
using Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    [UniqueComponent]
    public class SpriteRenderer : Renderer2D
    {
        public override bool FlipX
        {
            get => base.FlipX;
            set
            {
                if(base.FlipX == value)
                {
                    return;
                }

                base.FlipX = value;
                Sprite.Texture.Atlas.UpdateUvs(Sprite.AtlasIndex, QuadUV.FlipUV(Sprite.GetAtlasChunk().Uvs, value, FlipY));
                IsDirty = true;
            }
        }

        public override bool FlipY
        {
            get => base.FlipY;
            set
            {
                if (base.FlipY == value)
                {
                    return;
                }

                base.FlipY = value;
                Sprite.Texture.Atlas.UpdateUvs(Sprite.AtlasIndex, QuadUV.FlipUV(Sprite.GetAtlasChunk().Uvs, FlipX, value));
                IsDirty = true;
            }
        }
    }
}