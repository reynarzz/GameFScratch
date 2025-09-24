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
        private vec2 _offset = new vec2(0, 0);
        private static B2ShapeId InvalidShapeID = new B2ShapeId(-1, 0, 0);

        internal RigidBody2D RigidBody
        {
            get => _rigid;
            set
            {
                _rigid = value;
                _shapeDef.userData = value;
            }
        }

        public vec2 Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                RigidBody?.UpdateCollider(this);
            }
        }

        public float RotationOffset { get; set; } = 0;
        internal B2ShapeId ShapeID { get; private set; } = InvalidShapeID;
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
                RigidBody?.UpdateCollider(this);
            }
        }

        internal override void OnInitialize()
        {
            _rigid = GetComponent<RigidBody2D>();

            _shapeDef = new B2ShapeDef()
            {
                //enableContactEvents = true,
                //enableSensorEvents = false,
                //enableHitEvents = true,
                // enablePreSolveEvents = false,
                invokeContactCreation = true,
                isSensor = false,
                density = 1,
                updateBodyMass = true,
                material = B2Types.b2DefaultSurfaceMaterial(),
                filter = B2Types.b2DefaultFilter(),
                internalValue = B2Constants.B2_SECRET_COOKIE,
                userData = _rigid
            };

            _rigid?.AddCollider(this);
        }

        internal void Create(B2BodyId bodyId)
        {
            DestroyShape();
            ShapeID = CreateShape(bodyId, _shapeDef);
        }

        protected abstract B2ShapeId CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef);

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
                ShapeID = InvalidShapeID;
            }
        }

        
        public float Friction 
        { 
            get => B2Shapes.b2Shape_GetFriction(ShapeID);
            set
            {
                B2Shapes.b2Shape_SetFriction(ShapeID, value);
            }
        }         
    }
}