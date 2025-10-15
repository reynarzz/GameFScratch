using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public enum BlendFactor
    {
        Zero,
        One,
        SrcColor,
        OneMinusSrcColor,
        DstColor,
        OneMinusDstColor,
        SrcAlpha,
        OneMinusSrcAlpha,
        DstAlpha,
        OneMinusDstAlpha,
        ConstantColor,
        OneMinusConstantColor,
        ConstantAlpha,
        OneMinusConstantAlpha,
        SrcAlphaSaturate
    }

    public enum BlendEquation
    {
        FuncAdd,
        FuncSubtract,
        FuncReverseSubtract,
        Min,
        Max
    }

    public class Blending
    {
        public bool Enabled;

        public BlendFactor SrcFactor;
        public BlendFactor DstFactor;
        public BlendEquation Equation;

        public static Blending Transparent => new Blending() { Enabled = true, SrcFactor = BlendFactor.SrcAlpha, DstFactor = BlendFactor.OneMinusSrcAlpha };
        public static Blending Additive => new Blending() { Enabled = true, SrcFactor = BlendFactor.One, DstFactor = BlendFactor.One };
    }
}
