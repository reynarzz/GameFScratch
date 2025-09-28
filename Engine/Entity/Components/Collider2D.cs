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
        private bool _isTrigger = false;

        protected ref B2ShapeDef ShapeDef => ref _shapeDef;
        internal B2ShapeId[] ShapesId => _shapesID;
        private B2Filter _defaultFilter = new B2Filter(B2Constants.B2_DEFAULT_CATEGORY_BITS,
                                                       B2Constants.B2_DEFAULT_MASK_BITS, 0);

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                var canChange = value != base.IsEnabled;
                base.IsEnabled = value;

                if (canChange && AreShapesValid())
                {
                    ApplyToShapesSafe(shape =>
                    {
                        B2Shapes.b2Shape_SetFilter(shape, value ? _defaultFilter : default);
                    });
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
                var world = B2Worlds.b2GetWorldFromId(PhysicWorld.WorldID);

                var success = ApplyToShapesSafe(shapeid =>
                {
                    B2Shapes.b2Shape_EnableSensorEvents(shapeid, value);
                    B2Shapes.b2Shape_EnableContactEvents(shapeid, !value);
                    _shapeDef.isSensor = value;

                    var shape = B2Shapes.b2GetShape(world, shapeid);

                    if (value)
                    {
                        /* Note: Couldn't find a way to enable/disable sensors without destroying the shape
                                 So i copied the internal logic of box2d to mark a shape as 'sensor', this works correctly here.
                        */
                        shape.sensorIndex = world.sensors.count;
                        B2Sensor sensor = new B2Sensor
                        {
                            hits = B2Arrays.b2Array_Create<B2Visitor>(4),
                            overlaps1 = B2Arrays.b2Array_Create<B2Visitor>(16),
                            overlaps2 = B2Arrays.b2Array_Create<B2Visitor>(16),
                            shapeId = shape.id
                        };

                        B2Arrays.b2Array_Push(ref world.sensors, sensor);
                    }
                    else
                    {
                        shape.sensorIndex = B2Constants.B2_NULL_INDEX;
                    }
                });
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
                filter = _defaultFilter,
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