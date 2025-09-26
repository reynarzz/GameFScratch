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

            return [new B2ShapeId(-1, 0, 0)];
        }
    }
}
