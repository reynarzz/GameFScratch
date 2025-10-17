using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using FontStashSharp;
using GlmNet;

namespace Engine
{
    public class TextRenderer : Renderer
    {
        public FontAsset Font { get; set; }
        public StringBuilder Text { get; } = new StringBuilder();
        public Color Color { get; set; } = Color.White;
        public float FontSize { get; set; } = 32;
        public float CharacterSpacing { get; set; }
        public float LineSpacing { get; set; }
        public int OutlineSize { get; set; }
    }
}