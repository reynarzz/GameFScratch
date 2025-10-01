using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public struct UniformValue
    {
        public string Name { get; private set; }
        public int IntValue { get; private set; }
        public uint UIntValue { get; private set; }
        public float FloatValue { get; private set; }
        public mat4 Mat4Value { get; private set; }
        public vec2 Vec2Value { get; private set; }
        public vec3 Vec3Value { get; private set; }
        public int[] IntArrValue { get; private set; }
        public UniformType Type { get; private set; }

        public void SetInt(string name, int value)
        {
            Name = name;
            IntValue = value;
            Type = UniformType.Int;
        }

        public void SetUInt(string name, uint value)
        {
            Name = name;
            UIntValue = value;
            Type = UniformType.Uint;
        }

        public void SetFloat(string name, float value)
        {
            Name = name;
            FloatValue = value;
            Type = UniformType.Float;
        }

        public void SetMat4(string name, mat4 value)
        {
            Name = name;
            Mat4Value = value;
            Type = UniformType.Mat4;
        }

        public void SetVec2(string name, vec2 value)
        {
            Name = name;
            Vec2Value = value;
            Type = UniformType.Vec2;
        }

        public void SetVec3(string name, vec3 value)
        {
            Name = name;
            Vec3Value = value;
            Type = UniformType.Vec3;
        }

        public void SetIntArr(string name, int[] value)
        {
            Name = name;
            IntArrValue = value;
            Type = UniformType.IntArr;
        }
    }
}