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
        public RigidBody2D RigidBody { get; internal set; }
        public float RotationOffset { get; set; } = 0;

        private B2ShapeId _shapeID = InvalidShapeID;
        private B2ShapeDef _shapeDef;
        private vec2 _offset = new vec2(0, 0);
        private B2Filter _filter;

        private bool _isTrigger = false;
        private static B2ShapeId InvalidShapeID = new B2ShapeId(-1, 0, 0);

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                var canChange = value != base.IsEnabled;
                base.IsEnabled = value;

                if (canChange && B2Worlds.b2Shape_IsValid(_shapeID))
                {
                    if (value)
                    {
                        RigidBody.UpdateCollider(this);
                    }
                    else
                    {
                        DestroyShape();
                    }
                }
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
                B2Shapes.b2Shape_EnableSensorEvents(_shapeID, value);
                B2Shapes.b2Shape_EnableContactEvents(_shapeID, !value);
                _shapeDef.isSensor = value;
                RigidBody?.UpdateCollider(this);
            }
        }

        public float Friction
        {
            get => B2Shapes.b2Shape_GetFriction(_shapeID);
            set
            {
                B2Shapes.b2Shape_SetFriction(_shapeID, value);
            }
        }

        public float Bounciness
        {
            get => B2Shapes.b2Shape_GetRestitution(_shapeID);
            set
            {
                B2Shapes.b2Shape_SetRestitution(_shapeID, value);
            }
        }
        internal override void OnInitialize()
        {
            RigidBody = GetComponent<RigidBody2D>();
            _filter = B2Types.b2DefaultFilter();

            _shapeDef = new B2ShapeDef()
            {
                enableContactEvents = true,
                enableHitEvents = true,
                //enableSensorEvents = false,
                // enablePreSolveEvents = false,
                invokeContactCreation = true,
                isSensor = false,
                density = 1,
                updateBodyMass = true,
                material = B2Types.b2DefaultSurfaceMaterial(),
                filter = _filter,
                internalValue = B2Constants.B2_SECRET_COOKIE,
                userData = this
            };

            RigidBody?.AddCollider(this);
        }

        internal void Create(B2BodyId bodyId)
        {
            if (IsEnabled)
            {
                DestroyShape();
                _shapeID = CreateShape(bodyId, _shapeDef);
            }
        }

        protected abstract B2ShapeId CreateShape(B2BodyId bodyId, B2ShapeDef shapeDef);

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (RigidBody != null)
            {
                RigidBody.RemoveCollider(_shapeID);
                DestroyShape();
                RigidBody = null;
            }
        }

        private void DestroyShape()
        {
            if (B2Worlds.b2Shape_IsValid(_shapeID))
            {
                // TODO: destroy collection of shapes
                var autoMass = RigidBody ? RigidBody.IsAutoMass : false;
                B2Shapes.b2DestroyShape(_shapeID, autoMass);

                _shapeID = InvalidShapeID;
            }
        }
    }
}