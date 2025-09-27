using Box2D.NET;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public enum CapsuleDirection2D
    {
        Vertical, Horizontal
    }

    public class CapsuleCollider2D : Collider2D
    {
        private CapsuleDirection2D _direction = CapsuleDirection2D.Vertical;
        private vec2 _size = new vec2(1, 2);
        public vec2 Size
        {
            get => _size;
            set
            {
                _size = value;
                Create();
            }
        }

        public CapsuleDirection2D Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                Create();
            }
        }

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef)
        {
            var radius = _size.x / 2.0f;
            float rectHeight = MathF.Max(_size.y - 2 * radius, 0);

            var centerOffset = Direction == CapsuleDirection2D.Vertical
                ? new B2Vec2(0, rectHeight / 2.0f)
                : new B2Vec2(rectHeight / 2.0f, 0);

            var capsule = new B2Capsule()
            {
                radius = radius,
                center1 = centerOffset,
                center2 = -centerOffset, 
            };

            return [B2Shapes.b2CreateCapsuleShape(bodyId, ref shapeDef, ref capsule)];
        }
    }
}
