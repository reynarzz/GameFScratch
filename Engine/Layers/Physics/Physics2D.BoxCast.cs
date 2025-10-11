using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static partial class Physics2D
    {
        public static CastHit2D BoxCast(vec2 origin, vec2 size)
        {
            size = size * 0.5f;
            Box2dUtils.UpdateBox(ref _boxPolygon, size.x, size.y, origin);
            return default;
        }

    }
}
