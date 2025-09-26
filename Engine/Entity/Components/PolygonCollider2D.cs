using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utils.Polygon;
using Engine.Utils;
using GlmNet;

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
           var verts = new List<PVertex>
            {
                new PVertex(new Vec2(0, 0)),
                new PVertex(new Vec2(3, 1)),
                new PVertex(new Vec2(6, 0)),
                new PVertex(new Vec2(7, 2)),
                new PVertex(new Vec2(9, 1)),
                new PVertex(new Vec2(10, 4)),
                new PVertex(new Vec2(8, 5)),
                new PVertex(new Vec2(9, 8)),
                new PVertex(new Vec2(6, 9)),
                new PVertex(new Vec2(5, 6)),
                new PVertex(new Vec2(3, 8)),
                new PVertex(new Vec2(1, 7)),
                new PVertex(new Vec2(-1, 5)),
                new PVertex(new Vec2(-1, 2))
            };

            var poly = new ConcavePolygon(verts);

            poly.ConvexDecompBayazit();
            var pieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(pieces, 7);

            var shapes = new B2ShapeId[pieces.Count];
            var points = default(Vec2[]);
            var targetPoint = new B2Vec2[8];
            for (int i = 0; i < pieces.Count; i++)
            {
                int vertsCount = 0;
                pieces[i].GetPoints(ref points, ref vertsCount);

                for (int j = 0; j < pieces[i].GetPointCount(); j++)
                {
                    var p = points[j];
                    targetPoint[j] = new B2Vec2(p.x, p.y);
                }

                var hull = B2Hulls.b2ComputeHull(targetPoint, vertsCount);
                var polygon = B2Geometries.b2MakePolygon(ref hull, 0);
                shapes[i] = B2Shapes.b2CreatePolygonShape(bodyId, ref shapeDef, ref polygon);
            }

            return shapes;
        }
    }
}
