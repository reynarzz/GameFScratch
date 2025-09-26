using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct ContactPoint2D
    {
        public vec2 Position { get; internal set; }
        public vec2 Normal { get; internal set; }
        public float NormalImpulse { get; set; }
        public float TangentImpulse { get; set; }
        public float NormalVelocity { get; set; }
    }
}
