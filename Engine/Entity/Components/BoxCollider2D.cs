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
    public class BoxCollider2D : Collider2D
    {
        private vec2 _size = new vec2(1, 1);
        private float _cornerRadius = 0;

        public vec2 Size 
        {
            get => _size;
            set
            {
                _size = value;
                RigidBody?.UpdateCollider(this);
            }
        }

        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                RigidBody?.UpdateCollider(this);
            }
        }

        internal override void OnInitialize()
        {
            // TODO: instead of using the scale, use the bounds of the sprite
            var scale = Transform.WorldScale;
            _size = new vec2(scale.x, scale.y);

            base.OnInitialize();
        }

        protected override B2ShapeId CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef)
        {
            B2Polygon polygon = default;

            if(_cornerRadius > 0.001)
            {
                polygon = B2Geometries.b2MakeOffsetRoundedBox(Size.x / 2.0f, Size.y / 2.0f, Offset.ToB2Vec2(), glm.radians(RotationOffset).ToB2Rot(), _cornerRadius);
            }
            else
            {
                polygon = B2Geometries.b2MakeOffsetBox(Size.x / 2.0f, Size.y / 2.0f, Offset.ToB2Vec2(), glm.radians(RotationOffset).ToB2Rot());
            }

            return B2Shapes.b2CreatePolygonShape(bodyId, ref shapeDef, ref polygon);
        }
    }
}