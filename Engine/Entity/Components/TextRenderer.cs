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
    public enum TextVerticalAlignment
    {
        Center,
        Bottom,
        Top,
    }

    public enum TextHorizontalAlignment
    {
        Center,
        Left,
        Right,
    }

    public class TextRenderer : Renderer
    {
        public FontAsset Font { get; set; }
        public StringBuilder Text { get; } = new StringBuilder();
        public Color Color = Color.White;
        public float FontSize { get; set; } = 32;
        public float CharacterSpacing { get; set; }
        public float LineSpacing { get; set; }
        public int OutlineSize { get; set; }

        private TextVerticalAlignment _verticalAlignment = TextVerticalAlignment.Top;
        private TextHorizontalAlignment _horizontalAlignment = TextHorizontalAlignment.Left;
        public TextVerticalAlignment Vertical 
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment != value)
                {
                    IsDirty = true;
                }
                _verticalAlignment = value;

            }
        }

        public TextHorizontalAlignment Horizontal
        {
            get => _horizontalAlignment;
            set
            {
                if (_horizontalAlignment != value)
                {
                    IsDirty = true;
                }
                _horizontalAlignment = value;
            }
        }

        internal StringBuilder GetRenderableString()
        {
            return null;
        }
    }
}