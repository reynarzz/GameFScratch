using System;

namespace GlmNet
{
    public struct quat
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public quat Conjugate => new quat(-x, -y, -z, w);
        public static quat Identity => new quat(0, 0, 0, 1);
        public quat(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>Constructs quaternion from axis-angle (radians)</summary>
        public static quat FromAxisAngle(vec3 axis, float angle)
        {
            axis = glm.normalize(axis);
            float half = angle * 0.5f;
            float s = MathF.Sin(half);
            return new quat(axis.x * s, axis.y * s, axis.z * s, MathF.Cos(half));
        }

        /// <summary>Quaternion multiplication (combining rotations)</summary>
        public static quat operator *(quat a, quat b)
        {
            return new quat(
                a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
                a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
            );
        }

        /// <summary>Converts quaternion to 4x4 rotation matrix (column-major)</summary>
        public mat4 ToMat4()
        {
            float xx = x * x, yy = y * y, zz = z * z;
            float xy = x * y, xz = x * z, yz = y * z;
            float wx = w * x, wy = w * y, wz = w * z;

            mat4 m = mat4.identity();

            m[0, 0] = 1 - 2 * (yy + zz);
            m[0, 1] = 2 * (xy + wz);
            m[0, 2] = 2 * (xz - wy);

            m[1, 0] = 2 * (xy - wz);
            m[1, 1] = 1 - 2 * (xx + zz);
            m[1, 2] = 2 * (yz + wx);

            m[2, 0] = 2 * (xz + wy);
            m[2, 1] = 2 * (yz - wx);
            m[2, 2] = 1 - 2 * (xx + yy);

            return m;
        }

        /// <summary>Normalizes the quaternion</summary>
        public void Normalize()
        {
            float mag = MathF.Sqrt(x * x + y * y + z * z + w * w);
            if (mag > 0f)
            {
                float inv = 1.0f / mag;
                x *= inv;
                y *= inv;
                z *= inv;
                w *= inv;
            }
        }

        /// <summary>Converts quaternion to a vector representing Euler angles (radians)</summary>
        public vec3 ToEulerAngles()
        {
            vec3 angles;

            // roll (x-axis rotation)
            float sinr_cosp = 2 * (w * x + y * z);
            float cosr_cosp = 1 - 2 * (x * x + y * y);
            angles.x = MathF.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            float sinp = 2 * (w * y - z * x);
            if (MathF.Abs(sinp) >= 1)
                angles.y = MathF.CopySign(MathF.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.y = MathF.Asin(sinp);

            // yaw (z-axis rotation)
            float siny_cosp = 2 * (w * z + x * y);
            float cosy_cosp = 1 - 2 * (y * y + z * z);
            angles.z = MathF.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        /// <summary>Quaternion from Euler angles (radians)</summary>
        public static quat FromEulerAngles(vec3 euler)
        {
            float cy = MathF.Cos(euler.z * 0.5f);
            float sy = MathF.Sin(euler.z * 0.5f);
            float cp = MathF.Cos(euler.y * 0.5f);
            float sp = MathF.Sin(euler.y * 0.5f);
            float cr = MathF.Cos(euler.x * 0.5f);
            float sr = MathF.Sin(euler.x * 0.5f);

            return new quat(
                sr * cp * cy - cr * sp * sy,
                cr * sp * cy + sr * cp * sy,
                cr * cp * sy - sr * sp * cy,
                cr * cp * cy + sr * sp * sy
            );
        }
    }
}
