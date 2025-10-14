using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;
using Engine;

namespace Engine.Graphics.OpenGL
{
    internal static class GLHelpers
    {
        internal static int ToGL(this GfxValueType type) => type switch
        {
            GfxValueType.Float => GL_FLOAT,
            GfxValueType.Int => GL_INT,
            GfxValueType.Uint => GL_UNSIGNED_INT,
            GfxValueType.UByte => GL_UNSIGNED_BYTE,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown ValueType: {type}")
        };

        internal static int MapFunc(StencilFunc f)
        {
            switch (f)
            {
                case StencilFunc.Never: return GL_NEVER;
                case StencilFunc.Less: return GL_LESS;
                case StencilFunc.Lequal: return GL_LEQUAL;
                case StencilFunc.Greater: return GL_GREATER;
                case StencilFunc.Gequal: return GL_GEQUAL;
                case StencilFunc.Equal: return GL_EQUAL;
                case StencilFunc.NotEqual: return GL_NOTEQUAL;
                case StencilFunc.Always: return GL_ALWAYS;
            }
            return GL_ALWAYS;
        }

        internal static int MapOp(StencilOp op)
        {
            switch (op)
            {
                case StencilOp.Keep: return GL_KEEP;
                case StencilOp.Zero: return GL_ZERO;
                case StencilOp.Replace: return GL_REPLACE;
                case StencilOp.Incr: return GL_INCR;
                case StencilOp.IncrWrap: return GL_INCR_WRAP;
                case StencilOp.Decr: return GL_DECR;
                case StencilOp.DecrWrap: return GL_DECR_WRAP;
                case StencilOp.Invert: return GL_INVERT;
            }
            return GL_KEEP;
        }

        internal static int MapBlendFactor(BlendFactor f)
        {
            switch (f)
            {
                case BlendFactor.Zero: return GL_ZERO;
                case BlendFactor.One: return GL_ONE;
                case BlendFactor.SrcColor: return GL_SRC_COLOR;
                case BlendFactor.OneMinusSrcColor: return GL_ONE_MINUS_SRC_COLOR;
                case BlendFactor.DstColor: return GL_DST_COLOR;
                case BlendFactor.OneMinusDstColor: return GL_ONE_MINUS_DST_COLOR;
                case BlendFactor.SrcAlpha: return GL_SRC_ALPHA;
                case BlendFactor.OneMinusSrcAlpha: return GL_ONE_MINUS_SRC_ALPHA;
                case BlendFactor.DstAlpha: return GL_DST_ALPHA;
                case BlendFactor.OneMinusDstAlpha: return GL_ONE_MINUS_DST_ALPHA;
                case BlendFactor.ConstantColor: return GL_CONSTANT_COLOR;
                case BlendFactor.OneMinusConstantColor: return GL_ONE_MINUS_CONSTANT_COLOR;
                case BlendFactor.ConstantAlpha: return GL_CONSTANT_ALPHA;
                case BlendFactor.OneMinusConstantAlpha: return GL_ONE_MINUS_CONSTANT_ALPHA;
                case BlendFactor.SrcAlphaSaturate: return GL_SRC_ALPHA_SATURATE;
            }
            return GL_ONE;
        }

        internal static int MapBlendEquation(BlendEquation eq)
        {
            switch (eq)
            {
                case BlendEquation.FuncAdd: return GL_FUNC_ADD;
                case BlendEquation.FuncSubtract: return GL_FUNC_SUBTRACT;
                case BlendEquation.FuncReverseSubtract: return GL_FUNC_REVERSE_SUBTRACT;
                case BlendEquation.Min: return GL_MIN;
                case BlendEquation.Max: return GL_MAX;
            }
            return GL_FUNC_ADD;
        }
    }
}
