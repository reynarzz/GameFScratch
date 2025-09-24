using Box2D.NET;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class Collider2D : Component
    {
        private RigidBody2D _rigid;
        internal RigidBody2D RigidBody
        {
            get => _rigid;
            set
            {
                _rigid = value;
                _shapeDef.userData = value;
            }
        }

        public vec2 Offset { get; set; } = new vec2();
        public float RotationOffset { get; set; } = 0;
        internal B2ShapeId ShapeID { get; private set; } = new B2ShapeId(-1, 0, 0);
        private B2ShapeDef _shapeDef;

        private bool _isTrigger = false;
        public bool IsTrigger
        {
            get => _isTrigger;
            set
            {
                if (_isTrigger == value)
                {
                    return;
                }

                _isTrigger = value;
                _shapeDef.isSensor = value;
                if (RigidBody)
                {
                    RigidBody.UpdateCollider(this);
                }
            }
        }

        internal override void OnInitialize()
        {
            _rigid = GetComponent<RigidBody2D>();

            _shapeDef = new B2ShapeDef()
            {
                enableContactEvents = true,
                density = 1,
                enableSensorEvents = false,
                enableHitEvents = true,
                filter = B2Types.b2DefaultFilter(),
                userData = null,
                updateBodyMass = true
            };

            if (_rigid)
            {
                _shapeDef.userData = _rigid;
                _rigid.AddCollider(this);
            }
        }

        internal void Create(B2BodyId bodyId)
        {
            DestroyShape();
            var polygon = CreateShape();
            ShapeID = B2Shapes.b2CreatePolygonShape(bodyId, ref _shapeDef, ref polygon);
        }

        protected abstract B2Polygon CreateShape();

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_rigid)
            {
                _rigid.RemoveCollider(ShapeID);
                DestroyShape();
            }
        }

        private void DestroyShape()
        {
            if (ShapeID.index1 >= 0)
            {
                B2Shapes.b2DestroyShape(ShapeID, _rigid.IsAutoMass);
                ShapeID = new B2ShapeId(-1, 0, 0);
            }
        }
    }
}
