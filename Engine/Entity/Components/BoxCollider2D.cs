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
        public vec2 Size 
        {
            get 
            {
                return _size;
            }
            set
            {
                _size = value;
                RigidBody.UpdateCollider(this);
            }
        }

        protected override B2Polygon CreateShape()
        {
            return B2Geometries.b2MakeOffsetBox(Size.x / 2.0f, Size.y / 2.0f, Offset.ToB2Vec2(), glm.radians(RotationOffset).ToB2Rot());
        }
    }
}