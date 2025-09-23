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
    public enum RigidBody2DType
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
        private RigidBody2DType _bodyType = RigidBody2DType.Dynamic;
        private bool _isContinuos = false;
        private bool _canSleep = false;
        private bool _autoMass = false;
        private float _autoMassValue = 1.0f;
        private bool _isZRotationLocked = false;

        private B2ShapeId _defaultShapeId;

        public vec2 velocity
        {
            get => B2Bodies.b2Body_GetLinearVelocity(_bodyId).ToVec2();
            set => B2Bodies.b2Body_SetLinearVelocity(_bodyId, value.ToB2Vec2());
        }

        internal override void OnInitialize()
        {
            var worldPos = Transform.WorldPosition;
            var worldRot = Transform.WorldRotation;

            B2BodyDef bodyDef = default;
            bodyDef.type = (B2BodyType)_bodyType;
            bodyDef.position = new B2Vec2(worldPos.x, worldPos.y);
            bodyDef.rotation = worldRot.QuatToB2Rot();
            bodyDef.name = GetID().ToString();
            bodyDef.isBullet = false;
            bodyDef.fixedRotation = false;
            bodyDef.isAwake = true;
            bodyDef.isEnabled = true;
            bodyDef.gravityScale = 1;

            _bodyId = B2Bodies.b2CreateBody(PhysicWorld.WorldID, ref bodyDef);

            B2ShapeDef shapeDef = default;
            shapeDef.density = 1;
            shapeDef.enableContactEvents = false;
            shapeDef.enableHitEvents = true;
            shapeDef.enableSensorEvents = false;
            shapeDef.isSensor = false;
            shapeDef.filter.groupIndex = 0;
            shapeDef.filter.maskBits = 0;
            shapeDef.updateBodyMass = true;

            var defSquare = B2Geometries.b2MakeSquare(0.1f);
            _defaultShapeId = B2Shapes.b2CreatePolygonShape(_bodyId, ref shapeDef, ref defSquare);
        }

        internal void UpdateBody()
        {
            var position = B2Bodies.b2Body_GetPosition(_bodyId);

            Transform.WorldPosition = new vec3(position.X, position.Y, Transform.WorldPosition.z);
            Transform.WorldRotation = B2Bodies.b2Body_GetRotation(_bodyId).B2RotToQuat();
        }

        public void SetBodyType(RigidBody2DType type)
        {
            _bodyType = type;
            B2Bodies.b2Body_SetType(_bodyId, (B2BodyType)type);
        }

        public void SetIsContinuos(bool isContinuos)
        {
            _isContinuos = isContinuos;
            B2Bodies.b2Body_SetBullet(_bodyId, isContinuos);
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

        public void CanSleep(bool sleep)
        {
            _canSleep = sleep;
            B2Bodies.b2Body_EnableSleep(_bodyId, sleep);
        }

        public void AutoMass(bool autoMass)
        {
            _autoMass = autoMass;

            if (!autoMass)
            {
                SetMass(_autoMassValue);
            }
        }

        public void SetMass(float mass)
        {
            _autoMassValue = mass;

            if (_autoMass)
            {
                var currentMassData = B2Bodies.b2Body_GetMassData(_bodyId);
                currentMassData.mass = mass;
                B2Bodies.b2Body_SetMassData(_bodyId, currentMassData);
            }
        }

        public void LockZRotation(bool lockZ)
        {
            _isZRotationLocked = lockZ;
            B2Bodies.b2Body_SetFixedRotation(_bodyId, lockZ);
        }

        internal void AddCollider(Collider2D collider)
        {
            B2Shapes.b2DestroyShape(_defaultShapeId, true);
            
        }

        internal void RemoveCollider(Collider2D collider)
        {

        }
    }
}