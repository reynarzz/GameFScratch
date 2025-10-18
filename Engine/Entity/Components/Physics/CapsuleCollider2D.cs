using Box2D.NET;
using Engine.Utils;
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
                _size = new vec2(Math.Clamp(value.x, 1.0f, float.MaxValue), Math.Clamp(value.y, 1.001f, float.MaxValue));
                UpdateShape();
            }
        }

        public CapsuleDirection2D Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                UpdateShape();
            }
        }

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId)
        {
            var capsule = GetCapsule();

            return [B2Shapes.b2CreateCapsuleShape(bodyId, ref ShapeDef, ref capsule)];
        }

        private B2Capsule GetCapsule()
        {
            var radius = _size.x / 2.0f;
            float rectHeight = MathF.Max(_size.y - 2 * radius, 0);

            var centerOffset = Direction == CapsuleDirection2D.Vertical
                ? new B2Vec2(0, rectHeight / 2.0f)
                : new B2Vec2(rectHeight / 2.0f, 0);

            var rot = glm.radians(RotationOffset);

            float cos = MathF.Cos(rot);
            float sin = MathF.Sin(rot);

            B2Vec2 Rotate(B2Vec2 v)
            {
                return new B2Vec2(v.X * cos - v.Y * sin,
                                  v.X * sin + v.Y * cos);
            }

            var offset = new B2Vec2(Offset.x, Offset.y);

            return new B2Capsule()
            {
                radius = radius,
                center1 = Rotate(centerOffset) + offset,
                center2 = Rotate(-centerOffset) + offset,
            };
        }

        protected override void UpdateShape()
        {
            var capsule = GetCapsule();
            B2Shapes.b2Shape_SetCapsule(ShapesId[0], ref capsule);
        }
    }
}
