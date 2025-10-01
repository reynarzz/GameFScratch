using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public readonly struct UniformValue
    {
        public string Name { get; }
        public int IntValue { get; }
        public uint UIntValue { get; }
        public float FloatValue { get; }
        public mat4 Mat4Value { get; }
        public vec2 Vec2Value { get; }
        public vec3 Vec3Value { get; }

        public int[] IntArrValue { get; }

        public UniformType Type { get; }

        public UniformValue(string name, UniformType type, int intValue, uint uIntValue,
                                              float floatValue, mat4 mat4Value,
                                              vec2 vec2Value, vec3 vec3Value, int[] intArrValue)
        {
            Name = name;
            Type = type;
            IntValue = intValue;
            FloatValue = floatValue;
            Mat4Value = mat4Value;
            Vec2Value = vec2Value;
            Vec3Value = vec3Value;
            IntArrValue = intArrValue;
        }

        public static UniformValue AsInt(string name, int value) => new UniformValue(name, UniformType.Int, value, default, default, default, default, default, default);
        public static UniformValue AsUInt(string name, uint value) => new UniformValue(name, UniformType.Uint, default, value, default, default, default, default, default);
        public static UniformValue AsFloat(string name, float value) => new UniformValue(name, UniformType.Float, default, default, value, default, default, default, default);
        public static UniformValue AsMat4(string name, mat4 value) => new UniformValue(name, UniformType.Mat4, default, default, default, value, default, default, default);
        public static UniformValue AsVec2(string name, vec2 value) => new UniformValue(name, UniformType.Vec2, default, default, default, default, value, default, default);
        public static UniformValue AsVec3(string name, vec3 value) => new UniformValue(name, UniformType.Vec3, default, default, default, default, default, value, default);
        public static UniformValue AsIntArr(string name, int[] value) => new UniformValue(name, UniformType.IntArr, default, default, default, default, default, default, value);
    }
}