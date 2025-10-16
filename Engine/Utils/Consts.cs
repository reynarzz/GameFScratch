using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utils
{
    internal class Consts
    {
        internal const string VIEW_PROJ_UNIFORM_NAME = "uVP";
        internal const string VIEW_UNIFORM_NAME = "uViewMatrix";
        internal const string MODEL_UNIFORM_NAME = "uModel";
        internal const string TEX_ARRAY_UNIFORM_NAME = "uTextures";
        internal const string SCREEN_GRAB_TEX_UNIFORM_NAME = "uScreenGrabTex";
        internal const string SCREEN_SIZE_UNIFORM_NAME = "uScreenSize";
        internal const string FRAME_SEED_UNIFORM_NAME = "uFrameSeed";
        internal const string TIME_UNIFORM_NAME = "uTime";

        internal class Graphics
        {
            internal const int MAX_UNIFORMS_PER_DRAWCALL = 10;
            internal const int MAX_QUADS_PER_BATCH = 5000;

            internal enum Uniforms
            {
                VP_MATRIX,
                VIEW_MATRIX,
                TEXTURES_ARRAY,
                MODEL_MATRIX,
                SCREEN_RENDER_TARGET_GRAB,
                SCREEN_SIZE,
                APP_TIME,
            }
        }
    }
}
