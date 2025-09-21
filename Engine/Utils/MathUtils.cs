using GlmSharp;
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
            mat4 view = mat4.Identity;

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
            mat4 result = mat4.Identity;

            result[0, 0] = 2.0f / (right - left);
            result[1, 1] = 2.0f / (top - bottom);
            result[2, 2] = -2.0f / (far - near);

            result[3, 0] = -(right + left) / (right - left);
            result[3, 1] = -(top + bottom) / (top - bottom);
            result[3, 2] = -(far + near) / (far - near);

            return result;
        }

        public static mat4 Perspective(float fovY, float aspect, float near, float far)
        {
            float f = 1.0f / (float)Math.Tan(fovY / 2.0f);
            mat4 result = mat4.Identity;

            result[0, 0] = f / aspect;
            result[1, 1] = f;
            result[2, 2] = (far + near) / (near - far);
            result[2, 3] = -1.0f;
            result[3, 2] = (2.0f * far * near) / (near - far);

            return result;
        }
    }
}
