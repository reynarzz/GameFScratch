using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utils.Polygon;
using Engine.Utils;
using GlmNet;
using System.Runtime.InteropServices;

namespace Engine
{
    public class PolygonCollider2D : Collider2D
    {
        public vec2[] Points { get; set; } =
        [
            new vec2(0f, 0f),   
            new vec2(1f, 0f),   
            new vec2(0.5f, 1f)  
        ];

        public void Generate()
        {
            if (Points.Length > 0)
            {
                Create();
            }
        }
        private List<B2Hull> _hulls = new List<B2Hull>();

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId)
        {
            if (Points.Length == 0 || Points == null)
            {
                Debug.Error("Polygon collider has zero vertices, it cannot be created.");
                return null;
            }

            var verts = new List<PVertex>(Points.Length);
            _hulls.Clear();

            for (int i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                verts.Add(new PVertex(p.x, p.y));
            }

            var poly = new ConcavePolygon(verts);

            poly.ConvexDecompBayazit();
            var pieces = new List<ConcavePolygon>();
            poly.ReturnLowestLevelPolys(pieces, B2Constants.B2_MAX_POLYGON_VERTICES);

            var shapes = new B2ShapeId[pieces.Count];
            var points = default(Vec2[]);
            var piecePoints = new B2Vec2[B2Constants.B2_MAX_POLYGON_VERTICES];


            for (int i = 0; i < pieces.Count; i++)
            {
                int vertsCount = 0;
                pieces[i].GetPoints(ref points, ref vertsCount);

                for (int j = 0; j < vertsCount; j++)
                {
                    var p = points[j];
                    piecePoints[j] = new B2Vec2(p.x, p.y);
                }

                var hull = B2Hulls.b2ComputeHull(piecePoints, vertsCount);

                _hulls.Add(hull);
                var polygon = GetPolygon(ref hull);
                shapes[i] = B2Shapes.b2CreatePolygonShape(bodyId, ref ShapeDef, ref polygon);
            }

            return shapes;
        }

        private B2Polygon GetPolygon(ref B2Hull hull)
        {
           return B2Geometries.b2MakeOffsetPolygon(ref hull, Offset.ToB2Vec2(), B2MathFunction.b2MakeRot(RotationOffset));
        }

        protected override void UpdateShape()
        {
            var hulls = CollectionsMarshal.AsSpan(_hulls);

            for (int i = 0; i < hulls.Length; i++)
            {
                var polygon = GetPolygon(ref hulls[i]);
                B2Shapes.b2Shape_SetPolygon(ShapesId[i], ref polygon);
            }
        }
    }
}
