using Box2D.NET;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utils
{
    public static class MathUtils
    {
        public static mat4 InvertCameraTransform(mat4 model)
        {
            mat4 view = mat4.identity();

            // Transpose the 3x3 rotation (upper-left 3x3)
            view[0, 0] = model[0, 0];
            view[0, 1] = model[1, 0];
            view[0, 2] = model[2, 0];

            view[1, 0] = model[0, 1];
            view[1, 1] = model[1, 1];
            view[1, 2] = model[2, 1];

            view[2, 0] = model[0, 2];
            view[2, 1] = model[1, 2];
            view[2, 2] = model[2, 2];

            // Invert translation
            float tx = model[0, 3];
            float ty = model[1, 3];
            float tz = model[2, 3];

            view[3, 0] = -(view[0, 0] * tx + view[1, 0] * ty + view[2, 0] * tz);
            view[3, 1] = -(view[0, 1] * tx + view[1, 1] * ty + view[2, 1] * tz);
            view[3, 2] = -(view[0, 2] * tx + view[1, 2] * ty + view[2, 2] * tz);

            // Last row stays [0,0,0,1]
            view[3, 3] = 1.0f;

            return view;
        }

        public static mat4 Ortho(float left, float right, float bottom, float top, float near, float far)
        {
            mat4 result = mat4.identity();

            result[0, 0] = 2.0f / (right - left);  // X scale
            result[1, 1] = 2.0f / (top - bottom);  // Y scale
            result[2, 2] = 2.0f / (far - near);    // Z scale, forward-positive
            result[3, 0] = -(right + left) / (right - left);   // X translation
            result[3, 1] = -(top + bottom) / (top - bottom);   // Y translation
            result[3, 2] = -(far + near) / (far - near);       // Z translation

            return result;
        }

        public static mat4 Perspective(float fovY, float aspect, float near, float far)
        {
            float f = 1.0f / (float)Math.Tan(fovY / 2.0f);
            mat4 result = mat4.identity();

            result[0, 0] = f / aspect;
            result[1, 1] = f;
            result[2, 2] = (far + near) / (near - far);
            result[2, 3] = -1;
            result[3, 2] = (2 * far * near) / (near - far);

            return result;
        }

        internal static B2Rot QuatToB2Rot(this quat q)
        {
            float angle = MathF.Atan2(2f * (q.w * q.z + q.x * q.y),
                                      1f - 2f * (q.y * q.y + q.z * q.z));
            return new B2Rot(MathF.Cos(angle), MathF.Sin(angle));
        }

        internal static quat B2RotToQuat(this B2Rot rot)
        {
            var angle = MathF.Atan2(rot.s, rot.c);
            vec3 zAxis = new vec3(0f, 0f, 1f);
            return quat.FromAxisAngle(zAxis, angle);
        }

        internal static B2Vec2 ToB2Vec2(this vec2 vec)
        {
            return new B2Vec2(vec.x, vec.y);
        }

        internal static vec2 ToVec2(this B2Vec2 vec)
        {
            return new vec2(vec.X, vec.Y);
        }

    }
}
