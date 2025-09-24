using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2D.NET;
using Engine.Utils;
using GlmNet;

namespace Engine
{
    public enum Body2DType
    {
        Static,
        Kinematic,
        Dynamic
    }

    public enum ForceMode2D
    {
        Force,
        Impulse
    }

    public class RigidBody2D : Component
    {
        private B2BodyId _bodyId;
        private Body2DType _bodyType = Body2DType.Dynamic;
        private bool _isContinuos = false;
        private bool _canSleep = false;
        private bool _isAutoMass = false;
        private float _userMassValue = 1.0f;
        private bool _isZRotationLocked = false;

        private B2ShapeId _defaultShapeId;

        public vec2 Velocity
        {
            get => B2Bodies.b2Body_GetLinearVelocity(_bodyId).ToVec2();
            set => B2Bodies.b2Body_SetLinearVelocity(_bodyId, value.ToB2Vec2());
        }

        public bool CanSleep
        {
            get => _canSleep;
            set
            {
                if (_canSleep == value)
                    return;
                _canSleep = value;

                B2Bodies.b2Body_EnableSleep(_bodyId, _canSleep);
            }
        }

        public bool IsAutoMass
        {
            get => _isAutoMass;
            set
            {
                _isAutoMass = value;
                if (_isAutoMass)
                {
                    B2Bodies.b2Body_ApplyMassFromShapes(_bodyId);
                }
                else
                {
                    Mass = _userMassValue;
                }
            }
        }

        public float Mass
        {
            get => _userMassValue;
            set
            {
                if (_isAutoMass)
                    return;

                _userMassValue = value;

                var currentMassData = B2Bodies.b2Body_GetMassData(_bodyId);
                currentMassData.mass = _userMassValue;
                B2Bodies.b2Body_SetMassData(_bodyId, currentMassData);
            }
        }

        public bool LockZRotation
        {
            get => _isZRotationLocked;
            set
            {
                if (_isZRotationLocked == value)
                    return;

                B2Bodies.b2Body_SetMotionLocks(_bodyId, new B2MotionLocks() { angularZ = value });
            }
        }

        public Body2DType BodyType
        {
            get => _bodyType;
            set
            {
                if (_bodyType == value) return;
                _bodyType = value;

                B2Bodies.b2Body_SetType(_bodyId, (B2BodyType)_bodyType);
            }
        }

        public bool IsContinuos
        {
            get => _isContinuos;
            set
            {
                if (_isContinuos == value) return;
                _isContinuos = value;
                B2Bodies.b2Body_SetBullet(_bodyId, _isContinuos);
            }
        }

        internal override void OnInitialize()
        {
            var worldPos = Transform.WorldPosition;
            var worldRot = Transform.WorldRotation;

            var bodyDef = B2Types.b2DefaultBodyDef();
            bodyDef.type = (B2BodyType)_bodyType;
            bodyDef.position = new B2Vec2(worldPos.x, worldPos.y);
            bodyDef.rotation = worldRot.QuatToB2Rot();
            bodyDef.name = GetID().ToString();
            bodyDef.isBullet = false;
            bodyDef.isAwake = true;
            bodyDef.isEnabled = true;
            bodyDef.gravityScale = 1;
            
            _bodyId = B2Bodies.b2CreateBody(PhysicWorld.WorldID, ref bodyDef);

            var colliders = GetComponents<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.RigidBody = this;
                collider.Create(_bodyId);
            }

            //B2ShapeDef shapeDef = default;
            //shapeDef.density = 1;
            //shapeDef.enableContactEvents = true;
            //shapeDef.enableHitEvents = false;
            //shapeDef.enableSensorEvents = false;
            //shapeDef.isSensor = false;
            //shapeDef.filter.groupIndex = 0;
            //shapeDef.filter.maskBits = 0;
            //shapeDef.updateBodyMass = true;

            //var defSquare = B2Geometries.b2MakeSquare(0.1f);
            //_defaultShapeId = B2Shapes.b2CreatePolygonShape(_bodyId, ref shapeDef, ref defSquare);
        }

        internal void PreUpdateBody()
        {
            if (Transform.ChangedThisFrameTODO)
            {
                B2Bodies.b2Body_SetTransform(_bodyId, Transform.WorldPosition.ToB2Vec2(), Transform.WorldRotation.QuatToB2Rot());
            }
        }

        internal void PostUpdateBody()
        {
            var position = B2Bodies.b2Body_GetPosition(_bodyId);

            Transform.WorldPosition = new vec3(position.X, position.Y, Transform.WorldPosition.z);
            Transform.WorldRotation = B2Bodies.b2Body_GetRotation(_bodyId).B2RotToQuat();
        }

        public void AddForce(vec2 force, ForceMode2D mode)
        {
            switch (mode)
            {
                case ForceMode2D.Force:
                    B2Bodies.b2Body_ApplyForceToCenter(_bodyId, force.ToB2Vec2(), true);
                    break;
                case ForceMode2D.Impulse:
                    B2Bodies.b2Body_ApplyLinearImpulseToCenter(_bodyId, force.ToB2Vec2(), true);
                    break;
                default:
                    Log.Error($"ForceMode2D: '{mode}' Not implemented");
                    break;
            }
        }

        public void AddForce(vec2 force, vec2 point, ForceMode2D mode)
        {
            switch (mode)
            {
                case ForceMode2D.Force:
                    B2Bodies.b2Body_ApplyForce(_bodyId, point.ToB2Vec2(), force.ToB2Vec2(), true);
                    break;
                case ForceMode2D.Impulse:
                    B2Bodies.b2Body_ApplyLinearImpulse(_bodyId, point.ToB2Vec2(), force.ToB2Vec2(), true);
                    break;
                default:
                    Log.Error($"ForceMode2D: '{mode}' Not implemented");
                    break;
            }
        }

        internal void AddCollider(Collider2D collider)
        {
            collider.Create(_bodyId);
            Mass = _userMassValue;
        }

        internal void UpdateCollider(Collider2D collider)
        {
            collider.Create(_bodyId);
            Mass = _userMassValue;

        }

        internal void RemoveCollider(B2ShapeId shapeID)
        {

        }

        
    }
}