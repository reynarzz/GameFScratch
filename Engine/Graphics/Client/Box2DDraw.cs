using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class Box2DDraw
    {
        //private GfxResource ;

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

        internal static void DrawCircle(B2Vec2 center, float radius, B2HexColor color, object context)
        {

        }

        internal static void DrawPolygon(ReadOnlySpan<B2Vec2> vertices, int vertexCount, B2HexColor color, object context)
        {

        }

        internal static void DrawSolidPolygon(ref B2Transform transform, ReadOnlySpan<B2Vec2> vertices, int vertexCount, float radius, B2HexColor color, object context)
        {

        }

        internal static void DrawSolidCircle(ref B2Transform transform, float radius, B2HexColor color, object context)
        {

        }

        internal static void DrawSolidCapsule(B2Vec2 p1, B2Vec2 p2, float radius, B2HexColor color, object context)
        {

        }

        internal static void DrawSegment(B2Vec2 p1, B2Vec2 p2, B2HexColor color, object context)
        {

        }

        internal static void DrawTransform(B2Transform transform, object context)
        {

        }

        internal static void DrawPoint(B2Vec2 p, float size, B2HexColor color, object context)
        {

        }

        internal static void DrawString(B2Vec2 p, string s, B2HexColor color, object context)
        {

        }
    }
}
