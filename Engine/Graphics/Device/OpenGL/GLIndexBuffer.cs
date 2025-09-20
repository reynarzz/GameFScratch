using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLIndexBuffer : GLBuffer
    {
        public GLIndexBuffer() : base(GL_ELEMENT_ARRAY_BUFFER) { }
    }
}
