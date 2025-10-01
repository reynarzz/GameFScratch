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
        internal const string MODEL_UNIFORM_NAME = "uModel";
        internal const string TEX_ARRAY_UNIFORM_NAME = "uTextures";

        internal class Graphics
        {
            internal const int MAX_UNIFORMS_PER_DRAWCALL = 10;
            internal const int MAX_QUADS_PER_BATCH = 5000;

            internal const int VP_MATRIX_UNIFORM_INDEX = 0;
            internal const int TEXTURES_ARRAY_UNIFORM_INDEX = 1;
            internal const int MODEL_MATRIX_UNIFORM_INDEX = 2;
        }
    }
}
