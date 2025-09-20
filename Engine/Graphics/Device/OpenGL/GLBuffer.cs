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
        //internal enum GLBufferTarget : int
        //{
        //    GL_ARRAY_BUFFER = 0x8892, 
        //    GL_ELEMENT_ARRAY_BUFFER = 0x8893,
        //    GL_COPY_READ_BUFFER = 0x8F36,
        //    GL_COPY_WRITE_BUFFER = 0x8F37,
        //    GL_PIXEL_PACK_BUFFER = 0x88EB,
        //    GL_PIXEL_UNPACK_BUFFER = 0x88EC,
        //    GL_SHADER_STORAGE_BUFFER = 0x90D2,
        //    GL_UNIFORM_BUFFER = 0x8A11,
        //    GL_TRANSFORM_FEEDBACK_BUFFER = 0x8C8E,
        //    GL_TEXTURE_BUFFER = 0x8C2A,
        //    GL_DISPATCH_INDIRECT_BUFFER = 0x90EE,
        //    GL_DRAW_INDIRECT_BUFFER = 0x8F3F
        //}
        protected int Target { get; private set; }

        public GLBuffer(int target) : base(x => glBindBuffer(target, x),
                                                glGenBuffer,
                                                glDeleteBuffer)
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
                Logger.Error("Invalid buffer draw mode");
                return false;
            }

            if (desc.Size <= 0)
            {
                Logger.Error("Invalid buffer size");
                return false;
            }

            if (desc.Buffer == IntPtr.Zero) 
            {
                Logger.Error("Invalid buffer data (zero)");

                return false;
            }

            Bind();
            glBufferData(Target, desc.Size, desc.Buffer, usage);
            UnBind();

            return true;
        }

        public override void UpdateResource(BufferDataDescriptor desc)
        {
            Bind();
            glBufferSubData(Target, desc.Offset, desc.Size, desc.Buffer);
            UnBind();
        }
    }
}
