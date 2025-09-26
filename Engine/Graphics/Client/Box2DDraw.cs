using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Engine.Utils;


namespace Engine.Graphics
{
    internal class Box2DDraw
    {
        private static B2Transform DefaultTransform = new B2Transform() { p = default, q = new B2Rot(1, 0) };
        internal Box2DDraw()
        {
             
        }

        internal void Begin()
        {
            
        }

        internal void End()
        {
            // Draw all here in one go
        }

        
        internal static void DrawPolygon(ReadOnlySpan<B2Vec2> vertices, int vertexCount, B2HexColor color, object context)
        {
            DrawPolygonInternal(ref DefaultTransform, ref vertices, vertexCount, color);
        }

        internal static void DrawSolidPolygon(ref B2Transform transform, ReadOnlySpan<B2Vec2> vertices, int vertexCount, float radius, B2HexColor color, object context)
        {
            DrawPolygonInternal(ref transform, ref vertices, vertexCount, color);
        }

        private static void DrawPolygonInternal(ref B2Transform transform, ref ReadOnlySpan<B2Vec2> vertices, int vertexCount, B2HexColor color)
        {
            if (vertexCount < 2)
                return;

            for (int i = 0; i < vertexCount; i++)
            {
                B2Vec2 v0 = vertices[i];
                B2Vec2 v1 = vertices[(i + 1) % vertexCount];

                B2Vec2 tv0 = Mul(ref transform, v0);
                B2Vec2 tv1 = Mul(ref transform, v1);

                var p0 = new vec3(tv0.X, tv0.Y, 0);
                var p1 = new vec3(tv1.X, tv1.Y, 0);

                Debug.DrawLine(p0, p1, (uint)color);
            }
        }

        // Multiply a vector by a transform (rotation + translation)
        private static B2Vec2 Mul(ref B2Transform xf, B2Vec2 v)
        {
            // Rotate: x' = c*x - s*y, y' = s*x + c*y
            float x = xf.q.c * v.X - xf.q.s * v.Y;
            float y = xf.q.s * v.X + xf.q.c * v.Y;

            // Translate
            x += xf.p.X;
            y += xf.p.Y;

            return new B2Vec2(x, y);
        }

        internal static void DrawCircle(B2Vec2 center, float radius, B2HexColor color, object context)
        {
            Debug.DrawCircle(center.ToVec3(), radius, (uint)color);
        }

        internal static void DrawSolidCircle(ref B2Transform transform, float radius, B2HexColor color, object context)
        {
            var pos = Mul(ref transform, default);
            Debug.DrawCircle(pos.ToVec3(), radius, (uint)color);
        }

        internal static void DrawSolidCapsule(B2Vec2 p1, B2Vec2 p2, float radius, B2HexColor color, object context)
        {

        }

        internal static void DrawSegment(B2Vec2 p1, B2Vec2 p2, B2HexColor color, object context)
        {
            Debug.DrawLine(new vec3(p1.X, p1.Y, 0), new vec3(p2.X, p2.Y, 0), (uint)color);
        }

        internal static void DrawTransform(B2Transform transform, object context)
        {
            const float axisLength = 2;
            vec3 p = new vec3(transform.p.X, transform.p.Y, 0);

            vec3 xAxis = new vec3(transform.p.X + transform.q.c * axisLength,
                                  transform.p.Y + transform.q.s * axisLength, 0);

            vec3 yAxis = new vec3(transform.p.X - transform.q.s * axisLength,
                                  transform.p.Y + transform.q.c * axisLength, 0);

            // Draw axes
            Debug.DrawLine(p, xAxis, Color.Red);
            Debug.DrawLine(p, yAxis, Color.Green);
        }

        internal static void DrawPoint(B2Vec2 p, float size, B2HexColor color, object context)
        {

        }

        internal static void DrawString(B2Vec2 p, string s, B2HexColor color, object context)
        {

        }
    }
}
