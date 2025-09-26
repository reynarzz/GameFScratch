using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utils.Polygon;

namespace Engine
{
    public class PolygonCollider2D : Collider2D
    {
        public List<GlmNet.vec2> Points { get; } = new List<GlmNet.vec2>();

        public void Generate()
        {
            if (Points.Count > 0)
            {
                Create();
            }
        }

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef)
        {
            List<PVertex> verts = new List<PVertex>
            {
                new PVertex(new Vec2(0, 0)),
                new PVertex(new Vec2(4, 1)),
                new PVertex(new Vec2(8, 0)),
                new PVertex(new Vec2(6, 3)),
                new PVertex(new Vec2(8, 6)),
                new PVertex(new Vec2(4, 5)),
                new PVertex(new Vec2(0, 6)),
                new PVertex(new Vec2(2, 3)),
                new PVertex(new Vec2(1, 2)),
                new PVertex(new Vec2(3, 2))
            };

            var poly = new ConcavePolygon(verts);

            poly.ConvexDecompBayazit();
            var pieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(pieces, 7);

            var shapes = new B2ShapeId[pieces.Count];
            var points = default(Vec2[]);
            for (int i = 0; i < pieces.Count; i++)
            {
                B2Polygon polygon = default;
                polygon.count = pieces[i].GetPointCount();

                int vertsCount = 0;
                pieces[i].GetPoints(ref points, ref vertsCount);
                polygon.centroid = ComputeCentroid(points, vertsCount);
                
                for (int j = 0; j < polygon.count; j++)
                {
                    var p = points[j];
                    polygon.vertices[j] = new B2Vec2(p.x, p.y);
                }
#if DEBUG
                var hull = B2Hulls.b2ComputeHull(polygon.vertices.AsSpan(), polygon.vertices.Length);
                bool valid = B2Hulls.b2ValidateHull(ref hull);
                if (!valid)
                {
                    Debug.Error("Invalid polygon hull");
                }
#endif
                shapes[i] = B2Shapes.b2CreatePolygonShape(bodyId, ref shapeDef, ref polygon);
            }

            return shapes;
        }

        private B2Vec2 ComputeCentroid(ReadOnlySpan<Vec2> vertices, int count)
        {
            float area = 0f;
            float cx = 0f;
            float cy = 0f;

            for (int i = 0; i < count; ++i)
            {
                Vec2 p0 = vertices[i];
                Vec2 p1 = vertices[(i + 1) % count];

                float cross = p0.x * p1.y - p1.x * p0.y;
                area += cross;
                cx += (p0.x + p1.x) * cross;
                cy += (p0.y + p1.y) * cross;
            }

            area *= 0.5f;

            if (Math.Abs(area) < float.Epsilon)
                return new B2Vec2(0, 0); // degenerate polygon

            float inv6A = 1.0f / (6.0f * area);
            return new B2Vec2(cx * inv6A, cy * inv6A);
        }
    }
}
