using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLVertexBuffer : GLBuffer
    {
        internal GLVertexBuffer() : base(GL_ARRAY_BUFFER) { } 
    }
}
