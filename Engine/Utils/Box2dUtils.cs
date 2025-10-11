using Box2D.NET;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utils
{
    internal static class Box2dUtils
    {
        private static B2Transform _boxTransform = B2MathFunction.b2Transform_identity;
     
        internal static void UpdateBox(ref B2Polygon shape, float halfWidth, float halfHeight, vec2 center)
        {
            _boxTransform.p = center.ToB2Vec2();

            shape.count = 4;
            shape.vertices[0] = B2MathFunction.b2TransformPoint(ref _boxTransform, new B2Vec2(-halfWidth, -halfHeight));
            shape.vertices[1] = B2MathFunction.b2TransformPoint(ref _boxTransform, new B2Vec2(halfWidth, -halfHeight));
            shape.vertices[2] = B2MathFunction.b2TransformPoint(ref _boxTransform, new B2Vec2(halfWidth, halfHeight));
            shape.vertices[3] = B2MathFunction.b2TransformPoint(ref _boxTransform, new B2Vec2(-halfWidth, halfHeight));
            
            shape.radius = 0.0f;
            shape.centroid = _boxTransform.p;
        }

        internal static void ApplyBoxNormals(ref B2Polygon shape)
        {
            shape.normals[0] = B2MathFunction.b2RotateVector(B2MathFunction.b2Rot_identity, new B2Vec2(0.0f, -1.0f));
            shape.normals[1] = B2MathFunction.b2RotateVector(B2MathFunction.b2Rot_identity, new B2Vec2(1.0f, 0.0f));
            shape.normals[2] = B2MathFunction.b2RotateVector(B2MathFunction.b2Rot_identity, new B2Vec2(0.0f, 1.0f));
            shape.normals[3] = B2MathFunction.b2RotateVector(B2MathFunction.b2Rot_identity, new B2Vec2(-1.0f, 0.0f));
        }
    }
}
