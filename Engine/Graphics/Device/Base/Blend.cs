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
    }
}
