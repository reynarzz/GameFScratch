using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine
{
    public class Texture2D : Texture
    {
        public TextureAtlasData Atlas { get; } = new();
        public int PixelPerUnit { get; set; } = 32;

        public Texture2D(int width, int height, int channels, byte[] data) : base(width, height, channels, data)
        {
        }
    }
}
