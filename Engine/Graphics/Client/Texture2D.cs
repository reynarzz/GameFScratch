using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Texture2D : Texture
    {
        public TextureAtlasData Atlas { get; } = new();
        public int PixelPerUnit { get; set; } = 32;
    }
}
