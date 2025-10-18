using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct Particle
    {
        public vec2 Position;
        public vec2 Size;
        public vec2 Velocity;
        public float Rotation;
        public float AngularVelocity;
        public float Life;
        public float StartLife;
        public Color Color;
    }
}
