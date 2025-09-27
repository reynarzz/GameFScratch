using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct Mathf
    {
        public static float Clamp(float value, float min, float max)
           => value < min ? min : (value > max ? max : value);

        public static float Clamp01(float v) => Clamp(v, 0f, 1f);

        public static float Lerp(float a, float b, float t)
            => a + (b - a) * Clamp01(t);

        public static float Dot(vec2 a, vec2 b)
            => a.x * b.x + a.y * b.y;

        public static float Dot(vec3 a, vec3 b)
            => a.x * b.x + a.y * b.y + a.z * b.z;

        public static float Dot(quat a, quat b)
            => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

        public static quat Mul(quat q, float s)
            => new quat(q.x * s, q.y * s, q.z * s, q.w * s);

        public static quat Mul(float s, quat q)
            => new quat(q.x * s, q.y * s, q.z * s, q.w * s);

        public static quat Lerp(quat a, quat b, float t)
        {
            // (a*(1-t) + b*t).Normalized
            quat result = Add(Mul(a, 1 - t), Mul(b, t));

            result.Normalize();

            return result;
        }

        public static quat Slerp(quat a, quat b, float t)
        {
            float dot = Dot(a, b);

            // If dot < 0, invert one to take shortest path
            if (dot < 0.0f)
            {
                b = new quat(-b.x, -b.y, -b.z, -b.w);
                dot = -dot;
            }

            const float DOT_THRESHOLD = 0.9995f;
            if (dot > DOT_THRESHOLD)
            {
                // Very close - do linear and normalize
                quat result = Add(a, Mul(Sub(b, a), t));
                result.Normalize();
                return result;
            }

            float theta_0 = (float)System.Math.Acos(dot); // angle between input quats
            float theta = theta_0 * t;
            quat c = Sub(b, Mul(a, dot));
            c.Normalize(); // orthonormal basis

            float cosTheta = (float)System.Math.Cos(theta);
            float sinTheta = (float)System.Math.Sin(theta);

            return Add(Mul(a, cosTheta), Mul(c, sinTheta));
        }

        private static quat Add(quat a, quat b)
            => new quat(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

        private static quat Sub(quat a, quat b)
            => new quat(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

        public static vec2 Lerp(vec2 a, vec2 b, float t)
            => a + (b - a) * Clamp01(t);

        public static vec3 Lerp(vec3 a, vec3 b, float t)
            => a + (b - a) * Clamp01(t);
        public static vec2 MoveTowards(vec2 current, vec2 target, float maxDistanceDelta)
        {
            vec2 toVector = target - current;
            float dist = MathF.Sqrt(toVector.x * toVector.x + toVector.y * toVector.y);

            if (dist <= maxDistanceDelta || dist == 0f)
                return target;

            return current + toVector / dist * maxDistanceDelta;
        }

        public static vec3 MoveTowards(vec3 current, vec3 target, float maxDistanceDelta)
        {
            vec3 toVector = target - current;
            float dist = MathF.Sqrt(toVector.x * toVector.x + toVector.y * toVector.y + toVector.z * toVector.z);

            if (dist <= maxDistanceDelta || dist == 0f)
                return target;

            return current + toVector / dist * maxDistanceDelta;
        }

        public static float Distance(vec3 a, vec3 b)
        {
            vec3 c = a - b;
            return MathF.Sqrt(c.x * c.x + c.y * c.y + c.z * c.z);
        }

        public static float Distance(vec2 a, vec2 b)
        {
            vec2 c = a - b;
            return MathF.Sqrt(c.x * c.x + c.y * c.y);
        }

        public static vec2 Clamp(vec2 v, vec2 min, vec2 max)
            => new vec2(
                Clamp(v.x, min.x, max.x),
                Clamp(v.y, min.y, max.y));

        public static vec3 Clamp(vec3 v, vec3 min, vec3 max)
            => new vec3(
                Clamp(v.x, min.x, max.x),
                Clamp(v.y, min.y, max.y),
                Clamp(v.z, min.z, max.z));

        public static mat4 QuaternionToMat4(quat q)
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

            mat4 result = mat4.identity();

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
    }
}
