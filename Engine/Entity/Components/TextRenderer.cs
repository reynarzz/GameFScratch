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
        public string Text { get; set; }
        public Color Color { get; set; } = Color.White;
        public float FontSize { get; set; } = 32;
    }
}