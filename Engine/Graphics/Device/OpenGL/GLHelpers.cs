using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;


namespace Engine.Graphics.OpenGL
{
    internal static class GLHelpers
    {
        public static int ToGL(this GfxValueType type) => type switch
        {
            GfxValueType.Float => GL_FLOAT,
            GfxValueType.Int => GL_INT,
            GfxValueType.Uint => GL_UNSIGNED_INT,
            GfxValueType.UByte => GL_UNSIGNED_BYTE,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown ValueType: {type}")
        };
    }
}
