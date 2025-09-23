using Box2D.NET;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal static class PhysicWorld
    {
        public static B2WorldId WorldID { get; private set; }

        static PhysicWorld()
        {
            B2WorldDef worldDef = B2Types.b2DefaultWorldDef();
            worldDef.gravity = new B2Vec2(0, -9.8f);
            WorldID = B2Worlds.b2CreateWorld(ref worldDef);
        }

        internal static void SetGravity(vec2 gravity)
        {
            B2Worlds.b2World_SetGravity(WorldID, new B2Vec2(gravity.x, gravity.y));
        }
    }
}
