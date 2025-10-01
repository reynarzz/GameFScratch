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
        public ColorPacketRGBA PacketColor { get; set; } = Color.White;

        private Sprite _sprite;
        public Sprite Sprite 
        {
            get => _sprite;
            set
            {
                IsDirty = true;
                _sprite = value;
            }
        }



        public virtual bool FlipX { get; set; }
        public virtual bool FlipY { get; set; }
    }
}