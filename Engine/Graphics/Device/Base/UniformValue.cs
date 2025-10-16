using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal struct UniformValue
    {
        internal string Name { get; private set; }
        internal int IntValue { get; private set; }
        internal uint UIntValue { get; private set; }
        internal float FloatValue { get; private set; }
        internal mat4 Mat4Value { get; private set; }
        internal vec2 Vec2Value { get; private set; }
        internal vec3 Vec3Value { get; private set; }
        internal int[] IntArrValue { get; private set; }
        internal UniformType Type { get; private set; }

        internal void SetInt(string name, int value)
        {
            Name = name;
            IntValue = value;
            Type = UniformType.Int;
        }

        internal void SetUInt(string name, uint value)
        {
            Name = name;
            UIntValue = value;
            Type = UniformType.Uint;
        }

        internal void SetFloat(string name, float value)
        {
            Name = name;
            FloatValue = value;
            Type = UniformType.Float;
        }

        internal void SetMat4(string name, mat4 value)
        {
            Name = name;
            Mat4Value = value;
            Type = UniformType.Mat4;
        }

        internal void SetVec2(string name, vec2 value)
        {
            Name = name;
            Vec2Value = value;
            Type = UniformType.Vec2;
        }

        internal void SetVec3(string name, vec3 value)
        {
            Name = name;
            Vec3Value = value;
            Type = UniformType.Vec3;
        }

        internal void SetIntArr(string name, int[] value)
        {
            Name = name;
            IntArrValue = value;
            Type = UniformType.IntArr;
        }

        internal static UniformValue AsInt(string name, int value)
        {
            UniformValue val = default;
            val.SetInt(name, value);
            return val;
        }

        internal static UniformValue AsFloat(string name, float value)
        {
            UniformValue val = default;
            val.SetFloat(name, value);
            return val;
        }

        internal static UniformValue AsVec3(string name, vec3 value)
        {
            UniformValue val = default;
            val.SetVec3(name, value);
            return val;
        }

        internal static UniformValue AsVec2(string name, vec3 value)
        {
            UniformValue val = default;
            val.SetVec2(name, value);
            return val;
        }

        internal static UniformValue AsMat4(string name, mat4 value)
        {
            UniformValue val = default;
            val.SetMat4(name, value);
            return val;
        }
    }
}