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
        protected override B2ShapeId CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef)
        {
            List<PVertex> verts = new List<PVertex>
            {
                new PVertex(new Vec2(0, 0)),
                new PVertex(new Vec2(4, 0)),
                new PVertex(new Vec2(4, 2)),
                new PVertex(new Vec2(2, 1)), // concave VertexPolygon
                new PVertex(new Vec2(0, 2))
            };

            ConcavePolygon poly = new ConcavePolygon(verts);

            Log.Info("Original vertices:");
            for (int i = 0; i < poly.GetPointCount(); i++)
            {
                Vec2 p = poly.GetPoint(i);
                Log.Info($"  {i}: ({p.x}, {p.y})");
            }

            // Ear-clipping decomposition
            poly.ConvexDecomp();
            List<ConcavePolygon> earPieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(earPieces);

            Log.Info($"\nEar-clipping decomposition: {earPieces.Count} convex pieces");
            for (int i = 0; i < earPieces.Count; i++)
            {
                Log.Info($"Piece {i}:");
                for (int j = 0; j < earPieces[i].GetPointCount(); j++)
                {
                    Vec2 p = earPieces[i].GetPoint(j);
                    Log.Info($"  ({p.x}, {p.y})");
                }
            }

            // Bayazit decomposition
            poly.ConvexDecompBayazit();
            List<ConcavePolygon> bayazitPieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(bayazitPieces);

            Log.Info($"\nBayazit decomposition: {bayazitPieces.Count} convex pieces");
            for (int i = 0; i < bayazitPieces.Count; i++)
            {
                Log.Info($"Piece {i}:");
                for (int j = 0; j < bayazitPieces[i].GetPointCount(); j++)
                {
                    Vec2 p = bayazitPieces[i].GetPoint(j);
                    Log.Info($"  ({p.x}, {p.y})");
                }
            }

            Log.Info("\nDone.");

            return new B2ShapeId(-1, 0, 0);
        }
    }
}
