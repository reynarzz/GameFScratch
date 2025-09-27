using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2D.NET;
using Engine.Types;
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

    [UniqueComponent]
    public class RigidBody2D : Component
    {
        private B2BodyId _bodyId;
        private Body2DType _bodyType = Body2DType.Dynamic;
        private bool _isContinuos = false;
        private bool _canSleep = false;
        private bool _isAutoMass = false;
        private float _userMassValue = 1.0f;
        private bool _isZRotationLocked = false;
        private bool _shouldUpdatePreTransformation = false;
        private float _gravityScale = 1;
        private readonly static mat4 _identity = mat4.identity();
        internal B2BodyId BodyId => _bodyId;
        public vec2 Velocity
        {
            get => B2Bodies.b2Body_GetLinearVelocity(_bodyId).ToVec2();
            set => B2Bodies.b2Body_SetLinearVelocity(_bodyId, value.ToB2Vec2());
        }
        private bool _interpolate = false;
        public bool Interpolate
        {
            get => _interpolate;
            set
            {
                _interpolate = value;
                Transform.NeedsInterpolation = value;
            }
        }

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                if (value)
                {
                    B2Bodies.b2Body_Enable(_bodyId);
                }
                else
                {
                    B2Bodies.b2Body_Disable(_bodyId);
                }

                base.IsEnabled = value;
            }
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
                if (_isAutoMass || _bodyType != Body2DType.Dynamic)
                    return;

                _userMassValue = value;

                var currentMassData = B2Bodies.b2Body_GetMassData(_bodyId);
                currentMassData.mass = _userMassValue;
                B2Bodies.b2Body_SetMassData(_bodyId, currentMassData);
            }
        }

        public float GravityScale
        {
            get => _gravityScale;
            set
            {
                _gravityScale = value;
                B2Bodies.b2Body_SetGravityScale(_bodyId, _gravityScale);
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
                // if (_bodyType == value) return;
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

        internal vec3 PrevLocalPosition { get; set; }
        internal quat PrevLocalRotation { get; set; }

        internal void CalculatePhysicsInterpolation(Transform transform, vec3 prevPos, quat prevRot, float alpha)
        {
            // Parent world matrix (identity if no parent)
            mat4 parentMatrix = transform.Parent?.InterpolatedWorldMatrix ?? _identity;

            // Interpolated local transform
            vec3 localPos;
            quat localRot;

            if (Interpolate)
            {
                transform.NeedsInterpolation = true;
                // Interpolate in *local space*
                localPos = Mathf.Lerp(prevPos, transform.LocalPosition, alpha);
                localRot = Mathf.Slerp(prevRot, transform.LocalRotation, alpha);
            }
            else
            {
                localPos = transform.LocalPosition;
                localRot = transform.LocalRotation;
            }

            vec3 localScale = transform.LocalScale;

            // Build local matrix from interpolated local values
            mat4 scaleMat = glm.scale(mat4.identity(), localScale);
            mat4 rotMat = Mathf.QuatToMat4(localRot);
            mat4 transMat = glm.translate(mat4.identity(), localPos);

            mat4 localMatrix = transMat * rotMat * scaleMat;

            // Compose world matrix from parent
            transform.InterpolatedWorldMatrix = parentMatrix * localMatrix;

            foreach (var child in transform.Children)
            {
                child.NeedsInterpolation = Interpolate;
                CalculatePhysicsInterpolation(child.Transform, child.LocalPosition, child.LocalRotation, alpha);
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
            bodyDef.gravityScale = _gravityScale;
            bodyDef.enableSleep = false;

            _bodyId = B2Bodies.b2CreateBody(PhysicWorld.WorldID, ref bodyDef);

            var colliders = GetComponents<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.RigidBody = this;
                collider.Create();
            }

            Transform.OnChanged += OnTransformChanged;
        }

        internal void PreUpdateBody()
        {
            if (_shouldUpdatePreTransformation)
            {
                _shouldUpdatePreTransformation = false;
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
                    Debug.Error($"ForceMode2D: '{mode}' Not implemented");
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
                    Debug.Error($"ForceMode2D: '{mode}' Not implemented");
                    break;
            }
        }

        internal void UpdateBody()
        {
            Mass = _userMassValue;
        }

        private void OnTransformChanged(Transform transform)
        {
            _shouldUpdatePreTransformation = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (Transform)
            {
                foreach (var child in Transform.Children)
                {
                    child.NeedsInterpolation = false;
                }

                Transform.NeedsInterpolation = false;
                Transform.OnChanged -= OnTransformChanged;
            }
        }

        ~RigidBody2D()
        {
            B2Bodies.b2DestroyBody(_bodyId);
        }
    }
}