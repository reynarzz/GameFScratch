using System;
using System.Collections.Generic;
using GlmSharp;

namespace Engine
{
    public sealed class Transform : Component
    {
        // Local transform
        private vec3 _localPosition = vec3.Zero;
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
        private mat4 _cachedWorldMatrix = mat4.Identity;
        private vec3 _cachedWorldPosition = vec3.Zero;
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
        public mat4 LocalMatrix => mat4.Translate(_localPosition) * Rotate(_localRotation) * mat4.Scale(_localScale);

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
                    LocalPosition = (parentInv * new vec4(value, 1)).xyz;
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
                    LocalRotation = glm.Conjugate(Parent.WorldRotation) * value;
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
                if (Parent != null) LocalScale = value / Parent.WorldScale;
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
                    LocalRotation = glm.Conjugate(Parent.WorldRotation) * EulerToQuaternion(value);
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

            _cachedWorldPosition = new vec4(_cachedWorldMatrix[3]).xyz;
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
            euler.x = glm.Degrees((float)Math.Atan2(t0, t1));

            float t2 = +2.0f * (q.w * q.y - q.z * q.x);
            t2 = glm.Clamp(t2, -1.0f, 1.0f);
            euler.y = glm.Degrees((float)Math.Asin(t2));

            float t3 = +2.0f * (q.w * q.z + q.x * q.y);
            float t4 = +1.0f - 2.0f * (ysqr + q.z * q.z);
            euler.z = glm.Degrees((float)Math.Atan2(t3, t4));

            return euler;
        }

        private static quat EulerToQuaternion(vec3 euler)
        {
            float roll = glm.Radians(euler.x);
            float pitch = glm.Radians(euler.y);
            float yaw = glm.Radians(euler.z);

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

            mat4 scaleMat = mat4.Scale(invScale);
            mat4 rotMat = Rotate(glm.Conjugate(rotation));
            mat4 transMat = mat4.Translate(-position);

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

            mat4 result = mat4.Identity;

            result.m00 = 1 - 2 * (yy + zz);
            result.m01 = 2 * (xy + wz);
            result.m02 = 2 * (xz - wy);

            result.m10 = 2 * (xy - wz);
            result.m11 = 1 - 2 * (xx + zz);
            result.m12 = 2 * (yz + wx);

            result.m20 = 2 * (xz + wy);
            result.m21 = 2 * (yz - wx);
            result.m22 = 1 - 2 * (xx + yy);

            return result;
        }

        internal void RemoveChild(Transform transform)
        {
            _children.Remove(transform);
        }
    }
}
