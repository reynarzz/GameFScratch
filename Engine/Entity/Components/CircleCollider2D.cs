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
                RigidBody.UpdateCollider(this);
            }
        }

        protected override B2ShapeId CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef)
        {
            var circle = new B2Circle()
            {
                center = Offset.ToB2Vec2(),
                radius = _radius
            };

            return B2Shapes.b2CreateCircleShape(bodyId, ref shapeDef, ref circle);
        }
    }
}
