using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct Keyframe
    {
        public float Time;
        public float Value;
        public float InTangent;
        public float OutTangent;

        public Keyframe(float time, float value, float inTangent = 0f, float outTangent = 0f)
        {
            Time = time;
            Value = value;
            InTangent = inTangent;
            OutTangent = outTangent;
        }
    }
}
