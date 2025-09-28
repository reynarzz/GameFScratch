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

        private float _rotationOffset;
        public float RotationOffset
        {
            get => _rotationOffset;
            set
            {
                _rotationOffset = value;
                UpdateShape();
            }
        }

        private B2ShapeId[] _shapesID;
        private B2ShapeDef _shapeDef;
        private vec2 _offset = new vec2(0, 0);
        private B2Filter _filter;
        private bool _isTrigger = false;

        protected ref B2ShapeDef ShapeDef => ref _shapeDef;
        internal B2ShapeId[] ShapesId => _shapesID;

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                var canChange = value != base.IsEnabled;
                base.IsEnabled = value;

                if (canChange && AreShapesValid())
                {
                    if (value)
                    {
                        Create();
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
                UpdateShape();
            }
        }


        public bool IsTrigger
        {
            get => _isTrigger;
            set
            {
                if (/*_isTrigger == value ||*/ !AreShapesValid())
                {
                    return;
                }

                _isTrigger = value;
                var success = ApplyToShapesSafe(shapeid =>
                {
                    B2Shapes.b2Shape_EnableSensorEvents(shapeid, value);
                    B2Shapes.b2Shape_EnableContactEvents(shapeid, !value);
                });

                if (success)
                {
                    _shapeDef.isSensor = value;
                    Create();
                }
                else
                {
                    Debug.Error("Can't change trigger value to collider.");
                }
            }
        }

        public float Friction
        {
            get => AreShapesValid() ? B2Shapes.b2Shape_GetFriction(_shapesID[0]) : -1;
            set
            {
                ApplyToShapesSafe(shape =>
                {
                    B2Shapes.b2Shape_SetFriction(shape, value);
                });
            }
        }

        public float Bounciness
        {
            get => AreShapesValid() ? B2Shapes.b2Shape_GetRestitution(_shapesID[0]) : -1;
            set
            {
                ApplyToShapesSafe(shape =>
                {
                    B2Shapes.b2Shape_SetRestitution(shape, value);
                });
            }
        }

        internal override void OnInitialize()
        {
            RigidBody = GetComponent<RigidBody2D>();

            _shapeDef = new B2ShapeDef()
            {
                enableContactEvents = true,
                enableHitEvents = true,
                enableSensorEvents = true,
                // enablePreSolveEvents = false,
                invokeContactCreation = true,
                isSensor = false,
                density = 1,
                updateBodyMass = true,
                material = B2Types.b2DefaultSurfaceMaterial(),
                filter = B2Types.b2DefaultFilter(),
                internalValue = B2Constants.B2_SECRET_COOKIE,
                userData = this,
                enableCustomFiltering = true
            };



            Create();
        }

        internal void Create()
        {
            if (IsEnabled && RigidBody)
            {
                DestroyShape();
                _shapesID = CreateShape(RigidBody.BodyId);
                RigidBody.UpdateBody();
            }
        }

        protected abstract B2ShapeId[] CreateShape(B2BodyId bodyId);
        protected abstract void UpdateShape();

        protected bool AreShapesValid()
        {
            if (_shapesID == null || _shapesID.Length == 0)
                return false;

            for (int i = 0; i < _shapesID.Length; i++)
            {
                if (!B2Worlds.b2Shape_IsValid(_shapesID[i]))
                    return false;
            }

            return true;
        }

        private bool ApplyToShapesSafe(Action<B2ShapeId> shapeApply)
        {
            if (!AreShapesValid())
                return false;

            for (int i = 0; i < _shapesID.Length; i++)
            {
                shapeApply(_shapesID[i]);
            }

            return true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (RigidBody != null)
            {
                DestroyShape();
                RigidBody = null;
            }
        }

        private void DestroyShape()
        {
            if (_shapesID == null)
                return;

            for (int i = 0; i < _shapesID.Length; i++)
            {
                if (B2Worlds.b2Shape_IsValid(_shapesID[i]))
                {
                    var autoMass = RigidBody ? RigidBody.IsAutoMass : false;
                    B2Shapes.b2DestroyShape(_shapesID[i], autoMass);
                    _shapesID[i] = default;
                }
            }

            _shapesID = null;
        }
    }
}