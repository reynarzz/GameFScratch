using Box2D.NET;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct CastHit2D
    {
        public vec2 Point { get; internal set; }
        public vec2 Normal { get; internal set; }
        public Collider2D Collider { get; internal set; }
        public bool isHit { get; internal set; }
    }

    public static partial class Physics2D
    {
        public static bool DrawColliders { get; set; }
        private static readonly B2QueryFilter _defaultQueryFilter = new B2QueryFilter(B2Constants.B2_DEFAULT_CATEGORY_BITS, 
                                                                                      B2Constants.B2_DEFAULT_MASK_BITS);
        private static B2Polygon _boxPolygon;

        static Physics2D()
        {
            Box2dUtils.ApplyBoxNormals(ref _boxPolygon);
        }

        public static CastHit2D Raycast(vec2 origin, vec2 direction, ulong layerMask)
        {
            CastHit2D hit = default;
            float CastResultFunc(B2ShapeId shapeId, B2Vec2 point, B2Vec2 normal, float fraction, object context)
            {
                var collider = B2Shapes.b2Shape_GetUserData(shapeId) as Collider2D;
                
                if (!LayerMask.AreValid(collider.Actor.Layer, layerMask))
                {
                    return -1;
                }

                hit.Collider = collider;
                hit.Point = point.ToVec2();
                hit.Normal = normal.ToVec2();
                hit.isHit = true;

                return fraction; // Stop at the first hit
            }

            B2Worlds.b2World_CastRay(PhysicWorld.WorldID, origin.ToB2Vec2(), direction.ToB2Vec2(), _defaultQueryFilter, CastResultFunc, null);
            return hit;
        }

        public static CastHit2D Raycast(vec2 origin, vec2 direction)
        {
            return Raycast(origin, direction, ulong.MaxValue);
        }
    }
}