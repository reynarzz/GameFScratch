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
            if(Points.Count > 0)
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

            ConcavePolygon poly = new ConcavePolygon(verts);

            Debug.Info("Original vertices:");
            for (int i = 0; i < poly.GetPointCount(); i++)
            {
                Vec2 p = poly.GetPoint(i);
                Debug.Info($"  {i}: ({p.x}, {p.y})");
            }

            // Ear-clipping decomposition
            poly.ConvexDecomp();
            List<ConcavePolygon> earPieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(earPieces, 7);

            Debug.Info($"\nEar-clipping decomposition: {earPieces.Count} convex pieces");
            for (int i = 0; i < earPieces.Count; i++)
            {
                Debug.Info($"Piece {i}:");
                for (int j = 0; j < earPieces[i].GetPointCount(); j++)
                {
                    Vec2 p = earPieces[i].GetPoint(j);
                    Debug.Info($"  ({p.x}, {p.y})");
                }
            }

            // Bayazit decomposition
            poly.ConvexDecompBayazit();
            List<ConcavePolygon> bayazitPieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(bayazitPieces, 7);

            Debug.Info($"\nBayazit decomposition: {bayazitPieces.Count} convex pieces");
            for (int i = 0; i < bayazitPieces.Count; i++)
            {
                Debug.Info($"Piece {i}:");
                for (int j = 0; j < bayazitPieces[i].GetPointCount(); j++)
                {
                    Vec2 p = bayazitPieces[i].GetPoint(j);
                    Debug.Info($"  ({p.x}, {p.y})");
                }
            }

            Debug.Info("\nDone.");

            var shapes = new B2ShapeId[bayazitPieces.Count];
            var points = default(Vec2[]);
            int vertsCount = 0;
            for (int i = 0; i < bayazitPieces.Count; i++)
            {
                B2Polygon polygon = default;
                polygon.count = bayazitPieces[i].GetPointCount();
                 
                bayazitPieces[i].GetPoints(ref points, ref vertsCount);
                polygon.centroid = ComputeCentroid(points, vertsCount);

                for (int j = 0; j < polygon.count; j++)
                {
                    var p = bayazitPieces[i].GetPoint(j);
                    polygon.vertices[j] = new B2Vec2(p.x, p.y);
                }

                shapes[i] = B2Shapes.b2CreatePolygonShape(bodyId, ref shapeDef, ref polygon);
            }

            return shapes;
        }

        private B2Vec2 ComputeCentroid(Vec2[] vertices, int count)
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
