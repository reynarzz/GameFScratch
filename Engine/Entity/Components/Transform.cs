using System;
using System.Collections.Generic;
using GlmNet;

namespace Engine
{
    public sealed class Transform : Component
    {
        private static mat4 IdentityM => new mat4(1.0f);
        // Local transform
        private vec3 _localPosition = default;
        private quat _localRotation = quat.Identity;
        private vec3 _localScale = new vec3(1, 1, 1);
        private Transform _parent = null;

        public vec3 LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                MarkDirty();
            }
        }

        public quat LocalRotation
        {
            get => _localRotation;
            set
            {
                _localRotation = value;
                MarkDirty();
            }
        }

        public vec3 LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value; MarkDirty();
            }
        }

        // Hierarchy
        public Transform Parent
        {
            get => _parent;
            set
            {
                if (_parent == value)
                    return;

                if (value == null)
                {
                    // register as root
                    Actor.Scene.RegisterRootActor(Actor);
                }
                else
                {
                    // unregister as root
                    Actor.Scene.UnregisterRootActor(Actor);

                }
                _parent?._children.Remove(this);
                _parent = value;
                _parent?._children.Add(this);
                MarkDirty();
            }
        }

        private readonly List<Transform> _children = new List<Transform>();
        public IReadOnlyList<Transform> Children => _children.AsReadOnly();

        // Dirty flag and cached matrices
        private bool _isDirty = true;
        private mat4 _cachedWorldMatrix = IdentityM;
        private vec3 _cachedWorldPosition = default;
        private quat _cachedWorldRotation = quat.Identity;
        private vec3 _cachedWorldScale = new vec3(1, 1, 1);

        private void MarkDirty()
        {
            if (_isDirty)
                return;

            _isDirty = true;
            foreach (var child in _children)
            {
                child.MarkDirty();
            }
        }

        // Local matrix
        public mat4 LocalMatrix => glm.translate(IdentityM, _localPosition) * Rotate(_localRotation) * glm.scale(IdentityM, _localScale);

        // World transforms with lazy evaluation
        public mat4 WorldMatrix
        {
            get
            {
                UpdateWorldIfDirty();
                return _cachedWorldMatrix;
            }
        }

        public vec3 WorldPosition
        {
            get
            {
                UpdateWorldIfDirty();
                return _cachedWorldPosition;
            }
            set
            {
                if (Parent != null)
                {
                    mat4 parentInv = InverseTRS(Parent.WorldPosition, Parent.WorldRotation, Parent.WorldScale);
                    LocalPosition = new vec3(parentInv * new vec4(value, 1));
                }
                else LocalPosition = value;
            }
        }

        public quat WorldRotation
        {
            get
            {
                UpdateWorldIfDirty();
                return _cachedWorldRotation;
            }
            set
            {
                if (Parent != null)
                {
                    LocalRotation = Parent.WorldRotation.Conjugate * value;
                }
                else
                {
                    LocalRotation = value;
                }
            }
        }

        public vec3 WorldScale
        {
            get
            {
                UpdateWorldIfDirty();
                return _cachedWorldScale;
            }
            set
            {
                if (Parent != null) LocalScale = new vec3(value.x / Parent.WorldScale.x, value.y / Parent.WorldScale.y, value.z / Parent.WorldScale.z);
                else LocalScale = value;
            }
        }

        // Euler angles
        public vec3 LocalEulerAngles
        {
            get
            {
                return QuaternionToEuler(LocalRotation);
            }
            set
            {
                LocalRotation = EulerToQuaternion(value);
            }
        }

        public vec3 WorldEulerAngles
        {
            get => QuaternionToEuler(WorldRotation);
            set
            {
                if (Parent != null)
                    LocalRotation = Parent.WorldRotation.Conjugate * EulerToQuaternion(value);
                else
                    LocalRotation = EulerToQuaternion(value);
            }
        }

        // Helper to update world transforms if dirty
        private void UpdateWorldIfDirty()
        {
            if (!_isDirty) return;

            if (Parent != null)
            {
                _cachedWorldMatrix = Parent.WorldMatrix * LocalMatrix;
            }
            else _cachedWorldMatrix = LocalMatrix;

            _cachedWorldPosition = _cachedWorldPosition = new vec3(
            _cachedWorldMatrix[3, 0],
            _cachedWorldMatrix[3, 1],
            _cachedWorldMatrix[3, 2]
        );
            _cachedWorldRotation = Parent != null ? Parent.WorldRotation * LocalRotation : LocalRotation;
            _cachedWorldScale = Parent != null ? Parent.WorldScale * LocalScale : LocalScale;

            _isDirty = false;
        }

        // Quaternion <-> Euler helpers
        private static vec3 QuaternionToEuler(quat q)
        {
            vec3 euler;
            float ysqr = q.y * q.y;

            float t0 = +2.0f * (q.w * q.x + q.y * q.z);
            float t1 = +1.0f - 2.0f * (q.x * q.x + ysqr);
            euler.x = glm.degrees((float)Math.Atan2(t0, t1));

            float t2 = +2.0f * (q.w * q.y - q.z * q.x);
            t2 = Math.Clamp(t2, -1.0f, 1.0f);
            euler.y = glm.degrees((float)Math.Asin(t2));

            float t3 = +2.0f * (q.w * q.z + q.x * q.y);
            float t4 = +1.0f - 2.0f * (ysqr + q.z * q.z);
            euler.z = glm.degrees((float)Math.Atan2(t3, t4));

            return euler;
        }

        private static quat EulerToQuaternion(vec3 euler)
        {
            float roll = glm.radians(euler.x);
            float pitch = glm.radians(euler.y);
            float yaw = glm.radians(euler.z);

            float cy = (float)Math.Cos(yaw * 0.5f);
            float sy = (float)Math.Sin(yaw * 0.5f);
            float cp = (float)Math.Cos(pitch * 0.5f);
            float sp = (float)Math.Sin(pitch * 0.5f);
            float cr = (float)Math.Cos(roll * 0.5f);
            float sr = (float)Math.Sin(roll * 0.5f);

            return new quat(
                sr * cp * cy - cr * sp * sy,
                cr * sp * cy + sr * cp * sy,
                cr * cp * sy - sr * sp * cy,
                cr * cp * cy + sr * sp * sy
            );
        }

        // Inverse TRS for setting world positions
        private mat4 InverseTRS(vec3 position, quat rotation, vec3 scale)
        {
            vec3 invScale = new vec3(
                scale.x != 0 ? 1.0f / scale.x : 0,
                scale.y != 0 ? 1.0f / scale.y : 0,
                scale.z != 0 ? 1.0f / scale.z : 0
            );

            mat4 scaleMat = glm.scale(IdentityM, invScale);
            mat4 rotMat = Rotate(rotation.Conjugate);
            mat4 transMat = glm.translate(IdentityM, new vec3(-position.x, -position.y, -position.z));

            return scaleMat * rotMat * transMat;
        }

        private mat4 Rotate(quat q)
        {
            float x = q.x, y = q.y, z = q.z, w = q.w;

            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;
            float wx = w * x;
            float wy = w * y;
            float wz = w * z;

            mat4 result = IdentityM;

            // column 0
            result[0, 0] = 1 - 2 * (yy + zz);
            result[0, 1] = 2 * (xy + wz);
            result[0, 2] = 2 * (xz - wy);
            result[0, 3] = 0f;

            // column 1
            result[1, 0] = 2 * (xy - wz);
            result[1, 1] = 1 - 2 * (xx + zz);
            result[1, 2] = 2 * (yz + wx);
            result[1, 3] = 0f;

            // column 2
            result[2, 0] = 2 * (xz + wy);
            result[2, 1] = 2 * (yz - wx);
            result[2, 2] = 1 - 2 * (xx + yy);
            result[2, 3] = 0f;

            // column 3 (translation)
            result[3, 0] = 0f;
            result[3, 1] = 0f;
            result[3, 2] = 0f;
            result[3, 3] = 1f;

            return result;
        }

        internal void RemoveChild(Transform transform)
        {
            _children.Remove(transform);
        }
    }
}
