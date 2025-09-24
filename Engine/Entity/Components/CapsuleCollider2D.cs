using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class CapsuleCollider2D : Collider2D
    {
        private float _radius = 1;
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
            B2Capsule capsule = new B2Capsule()
            {
                radius = _radius
            };

            return B2Shapes.b2CreateCapsuleShape(bodyId, ref shapeDef, ref capsule);
        }
    }
}
