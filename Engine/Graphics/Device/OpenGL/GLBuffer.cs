using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLBuffer : GLGfxResource<BufferDataDescriptor>
    {
        protected int Target { get; private set; }

        public GLBuffer(int target) : base(glGenBuffer,
                                           glDeleteBuffer,
                                           handle => glBindBuffer(target, handle))
        {
            Target = target;
        }

        protected override bool CreateResource(BufferDataDescriptor desc)
        {
            int usage = desc.Usage switch
            {
                BufferUsage.Static => GL_STATIC_DRAW,
                BufferUsage.Dynamic => GL_DYNAMIC_DRAW,
                BufferUsage.Stream => GL_STREAM_DRAW,
                BufferUsage.Invalid => 0,
                _ => 0
            };

            if (usage == 0)
            {
                Log.Error("Invalid buffer draw mode");
                return false;
            }

            if (desc.Buffer == null || desc.Buffer.Length == 0)
            {
                Log.Error("Invalid buffer data (zero/null)");

                return false;
            }

            Bind();

            unsafe
            {
                fixed (byte* data = desc.Buffer)
                {
                    glBufferData(Target, desc.Buffer.Length, data, usage);
                }
            }
            Unbind();

            return true;
        }

        internal override void UpdateResource(BufferDataDescriptor desc)
        {
            Bind();
            unsafe
            {
                fixed (byte* data = desc.Buffer)
                {
                    glBufferSubData(Target, desc.Offset, desc.Buffer.Length, data);
                }
            }
            Unbind();
        }
    }
}