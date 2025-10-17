using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;


namespace Engine.Graphics.OpenGL
{
    internal class GLTexture : GLGfxResource<TextureDescriptor>
    {
        private int _slotBound = 0;
        public GLTexture() : base(glGenTexture, glDeleteTexture)
        {
        }

        protected unsafe override bool CreateResource(TextureDescriptor descriptor)
        {
            Bind();

            fixed (byte* data = descriptor.Buffer) 
            {
                // Upload data
                glTexImage2D(
                    GL_TEXTURE_2D,
                    0,                  // mip level
                    GL_RGBA,            // internal format
                    descriptor.Width,
                    descriptor.Height,
                    0,                  // border
                    GL_RGBA,            // format
                    GL_UNSIGNED_BYTE,   // type
                    data
                );
            }
            
            // Set default filtering/wrapping
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

            int texMode = 0;
            switch (descriptor.Mode)
            {
                case TextureMode.Clamp:
                    texMode = GL_CLAMP_TO_EDGE;
                    break;
                case TextureMode.Repeat:
                    texMode = GL_REPEAT;
                    break;
                default:
                    break;
            }

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, texMode);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, texMode);

            Unbind();

            return true;
        }

        internal override unsafe void UpdateResource(TextureDescriptor descriptor) 
        {
            Bind();
            fixed (void* data = descriptor.Buffer)
            {
                glTexSubImage2D(GL_TEXTURE_2D, 0, descriptor.XOffset, descriptor.YOffset, descriptor.Width, descriptor.Height, GL_RGBA, GL_UNSIGNED_BYTE, data);
            }
            Unbind();
        }

        /// <summary>
        /// Binds texture to first slot (0)
        /// </summary>
        internal override void Bind() 
        {
            Bind(0);
        }

        internal void Bind(int slot)
        {
            _slotBound = slot;
            glActiveTexture(GL_TEXTURE0 + slot);
            glBindTexture(GL_TEXTURE_2D, Handle);
        }

        internal override void Unbind()
        {
            glActiveTexture(GL_TEXTURE0 + _slotBound);
            glBindTexture(GL_TEXTURE_2D, 0);
            _slotBound = 0;
        }
    }
}