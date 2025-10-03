using System;
using System.Runtime.InteropServices;
using Engine.Graphics.OpenGL;
using static OpenGL.GL;

namespace Engine.Graphics
{
    internal class GLFrameBuffer : GLGfxResource<RenderTargetDescriptor>
    {
        private uint ColorTexture;
        private uint DepthStencilRBO;
        private int _width;
        private int _height;
        public int Width => _width;
        public int Height => _height;

        public GLFrameBuffer() : base(glGenFramebuffer,
                                    glDeleteFramebuffer,
                                    handle => glBindFramebuffer(GL_FRAMEBUFFER, handle))
        {
        }

        /// <summary>
        /// Blit the low-resolution framebuffer to the screen with nearest-neighbor scaling.
        /// </summary>
        internal void BlitToScreen(int windowWidth, int windowHeight)
        {
            glBindFramebuffer(GL_READ_FRAMEBUFFER, Handle);
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
            glBlitFramebuffer(0, 0, _width, _height, 0, 0, windowWidth, windowHeight,
                                 GL_COLOR_BUFFER_BIT, GL_NEAREST);
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
        }

        /// <summary>
        /// Read pixels from the framebuffer (useful for screenshots or pixel effects)
        /// </summary>
        internal byte[] ReadPixels()
        {
            byte[] pixels = new byte[Width * Height * 4]; // RGBA8
            glBindFramebuffer(GL_FRAMEBUFFER, Handle);
            GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            glReadPixels(0, 0, Width, Height, GL_RGBA, GL_UNSIGNED_BYTE, handle.AddrOfPinnedObject());
            handle.Free();
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
            return pixels;
        }

        protected override bool CreateResource(RenderTargetDescriptor descriptor)
        {
            _width = descriptor.Width;
            _height = descriptor.Height;

            Bind();
            // Create color texture (nearest-neighbor filtering)
            ColorTexture = glGenTexture();
            glBindTexture(GL_TEXTURE_2D, ColorTexture);
            glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA8, descriptor.Width, descriptor.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, IntPtr.Zero);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_NEAREST); // MIN_FILTER
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_NEAREST); // MAG_FILTER
            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, ColorTexture, 0);

            // Depth + stencil
            DepthStencilRBO = glGenRenderbuffer();
            glBindRenderbuffer(DepthStencilRBO);
            glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, descriptor.Width, descriptor.Height);
            glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, DepthStencilRBO);

            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                Debug.Error("Framebuffer is not complete!");
                return false;
            }

            Unbind();
            return true;
        }

        internal override void UpdateResource(RenderTargetDescriptor descriptor)
        {
            glDeleteTexture(ColorTexture);
            glDeleteRenderbuffer(DepthStencilRBO);

            CreateResource(descriptor);
        }
    }
}
