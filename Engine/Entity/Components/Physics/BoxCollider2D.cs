using Box2D.NET;
using Engine.Types;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    [RequiredComponent(typeof(RigidBody2D))]
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
                UpdateShape();
            }
        }

        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                UpdateShape();
            }
        }

        internal override void OnInitialize()
        {
            // TODO: instead of using the scale, use the bounds of the sprite
            var scale = Transform.WorldScale;
            _size = new vec2(scale.x, scale.y);

            var renderer = GetComponent<SpriteRenderer>();

            if (renderer && renderer.Sprite)
            {
                var chunk = renderer.Sprite.GetAtlasChunk();

                if (renderer.Sprite.Texture)
                {
                    var ppu = renderer.Sprite.Texture.PixelPerUnit;
                    var width = (float)chunk.Width / ppu;
                    var height = (float)chunk.Height / ppu;

                    _size = new vec2(width * scale.x, height * scale.y);
                }
            }


            base.OnInitialize();
        }

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId)
        {
            var polygon = GetPolygon();
            return [B2Shapes.b2CreatePolygonShape(bodyId, ref ShapeDef, ref polygon)];
        }

        private B2Polygon GetPolygon()
        {
            if (_cornerRadius > 0.001)
            {
                return B2Geometries.b2MakeOffsetRoundedBox(Size.x / 2.0f, Size.y / 2.0f, Offset.ToB2Vec2(), glm.radians(RotationOffset).ToB2Rot(), _cornerRadius);
            }
            return B2Geometries.b2MakeOffsetBox(Size.x / 2.0f, Size.y / 2.0f, Offset.ToB2Vec2(), glm.radians(RotationOffset).ToB2Rot());
        }

        protected override void UpdateShape()
        {
            var polygon = GetPolygon();
            B2Shapes.b2Shape_SetPolygon(ShapesId[0], ref polygon);
        }
    }
}