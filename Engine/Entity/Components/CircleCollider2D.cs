using Box2D.NET;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class CircleCollider2D : Collider2D
    {
        private float _radius = 0.5f;
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateShape();
            }
        }

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId)
        {
            var circle = GetCircle();
            return [B2Shapes.b2CreateCircleShape(bodyId, ref ShapeDef, ref circle)];
        }

        private B2Circle GetCircle()
        {
            return new B2Circle()
            {
                center = Offset.ToB2Vec2(),
                radius = _radius
            };
        }

        protected override void UpdateShape()
        {
            var circle = GetCircle();
            B2Shapes.b2Shape_SetCircle(ShapesId[0], ref circle);
        }
    }
}
