using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class Renderer2D : Renderer
    {
        public int SortOrder { get; set; } = 0;
        public Color Color { get => PacketColor; set => PacketColor = value; }
        public ColorPacketRGBA PacketColor { get; set; }
        public Sprite Sprite { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
    }
}