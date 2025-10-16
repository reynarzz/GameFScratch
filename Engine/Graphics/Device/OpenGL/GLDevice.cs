using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLDevice : GfxDevice
    {
        private readonly GfxDeviceInfo _gfxDeviceInfo;
        public GLDevice()
        {
            int maxTextureUnits;
            int maxTextureUnitsAccessInVertexShader;

            unsafe
            {
                glGetIntegerv(GL_MAX_TEXTURE_IMAGE_UNITS, &maxTextureUnits);
                glGetIntegerv(GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS, &maxTextureUnitsAccessInVertexShader);
            }

            _gfxDeviceInfo.MaxHardwareTextureUnits = maxTextureUnits;
            _gfxDeviceInfo.MaxTexAccessInVertexShader = maxTextureUnitsAccessInVertexShader;

            _gfxDeviceInfo.MaxValidTextureUnits = Math.Min(_gfxDeviceInfo.MaxHardwareTextureUnits, _gfxDeviceInfo.MaxTexAccessInVertexShader);
            _gfxDeviceInfo.Vendor = glGetString(GL_VENDOR);
            _gfxDeviceInfo.Renderer = glGetString(GL_RENDERER);
            _gfxDeviceInfo.Version = glGetString(GL_VERSION);
        }

        internal override void Initialize()
        {
        }

        internal override void Close()
        {
        }

        internal override void Clear(ClearDeviceConfig config)
        {
            var target = config.RenderTarget as GLFrameBuffer;

            if (target != null)
            {
                target.Bind();
            }
            glClearColor(config.Color.R, config.Color.G, config.Color.B, config.Color.A);
            glClearStencil(0);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);

            if (target != null)
                glViewport(0, 0, target.Width, target.Height);

            if (target != null)
            {
                target.Unbind();
            }
        }

        internal override GfxResource CreateGeometry(GeometryDescriptor desc)
        {
            GLGeometry geometry = new GLGeometry();
            geometry.Create(desc);
            return geometry;
        }

        internal override GfxResource CreateVertexBuffer(VertexDataDescriptor desc)
        {
            // TODO: Also create the vao here
            var vertexBuffer = new GLVertexBuffer();
            vertexBuffer.Create(desc.BufferDesc);

            return vertexBuffer;
        }

        internal override GfxResource CreateIndexBuffer(BufferDataDescriptor desc)
        {
            var indexBuffer = new GLIndexBuffer();
            indexBuffer.Create(desc);

            return indexBuffer;
        }

        internal override GfxResource CreateShader(ShaderDescriptor desc)
        {
            var shader = new GLShader();
            shader.Create(desc);
            return shader;
        }

        internal override GfxResource CreateTexture(TextureDescriptor desc)
        {
            var texture = new GLTexture();
            texture.Create(desc);
            return texture;
        }

        internal override GfxResource CreateRenderTarget(RenderTargetDescriptor desc)
        {
            var fbuffer = new GLFrameBuffer();
            fbuffer.Create(desc);
            return fbuffer;
        }

        private void DrawIndexed(DrawMode mode, int indicesLength)
        {
            unsafe
            {
                glDrawElements(GetGLDrawMode(mode), indicesLength, GL_UNSIGNED_INT, null);
            }
        }

        internal override void DrawArrays(DrawMode mode, int startIndex, int vertexCount)
        {
            glDrawArrays(GetGLDrawMode(mode), startIndex, vertexCount);
        }

        private int GetGLDrawMode(DrawMode mode)
        {
            var internalMode = mode switch
            {
                DrawMode.Triangles => GL_TRIANGLES,
                DrawMode.Lines => GL_LINES,
                DrawMode.Points => GL_POINTS,
                DrawMode.LineStrips => GL_LINE_STRIP,
                _ => -1
            };

            if (internalMode == -1)
            {
                throw new NotImplementedException($"Draw mode unsupported: {mode}");
            }

            return internalMode;
        }

        internal override void UpdateResouce(GfxResource resource, IGfxResourceDescriptor desc)
        {
            if (resource as GLGeometry != null)
            {
                (resource as GLGeometry).UpdateResource(desc as GeometryDescriptor);
            }
            else if (resource is GLFrameBuffer buffer)
            {
                buffer.UpdateResource(desc as RenderTargetDescriptor);
            }
        }

        internal override GfxDeviceInfo GetDeviceInfo()
        {
            return _gfxDeviceInfo;
        }

        internal override void UpdateGeometry(GfxResource resource, GeometryDescriptor desc)
        {
            (resource as GLGeometry).UpdateResource(desc);
        }

        internal override void SetViewport(vec4 viewport)
        {
            glViewport((int)viewport.x, (int)viewport.y, (int)viewport.z, (int)viewport.w);
        }

        internal override void Present()
        {
            Window.SwapBuffers();
        }

        internal override void Present(GfxResource renderTarget)
        {
            var frameBuffer = renderTarget as GLFrameBuffer;
            if (frameBuffer != null)
            {
                frameBuffer.BlitToScreen(Window.Width, Window.Height);
            }

            Present();
        }


        private void SetPipelineFeatures(PipelineFeatures features)
        {
            if (features.Blending.Enabled)
            {
                glEnable(GL_BLEND);
                glBlendFunc(GLHelpers.MapBlendFactor(features.Blending.SrcFactor), GLHelpers.MapBlendFactor(features.Blending.DstFactor));
                glBlendEquation(GLHelpers.MapBlendEquation(features.Blending.Equation));
            }
            else
            {
                glDisable(GL_BLEND);
            }

            if (features.Stencil.Enabled)
            {
                glEnable(GL_STENCIL_TEST);
                glStencilMask(0xFF);
                glStencilFunc(GLHelpers.MapFunc(features.Stencil.Func), features.Stencil.Ref, features.Stencil.Mask);
                glStencilOp(GLHelpers.MapOp(features.Stencil.FailOp), GLHelpers.MapOp(features.Stencil.ZFailOp), GLHelpers.MapOp(features.Stencil.ZPassOp));
            }
            else
            {
                glDisable(GL_STENCIL_TEST);
            }
        }

        internal override void Draw(DrawCallData drawCallData)
        {
            (drawCallData.Geometry as GLGeometry).Bind();
            var shader = drawCallData.Shader as GLShader;
            shader.Bind();

            int textureIndex = 0;
            for (; textureIndex < drawCallData.Textures.Length; textureIndex++)
            {
                var tex = drawCallData.Textures[textureIndex];
                if (tex == null)
                    break;
                (tex as GLTexture).Bind(textureIndex);
            }

            foreach (var (uniform, texture) in drawCallData.NamedTextures)
            {
                shader.SetUniform(uniform, textureIndex);

                (texture as GLTexture).Bind(textureIndex);

                textureIndex++;
            }

            var renderTarget = drawCallData.RenderTarget as GLFrameBuffer;

            if (renderTarget != null)
            {
                renderTarget.Bind();
            }
            SetViewport(drawCallData.Viewport);
            GLHelpers.SetUniforms(shader, drawCallData.Uniforms);
            SetPipelineFeatures(drawCallData.Features);

            switch (drawCallData.DrawType)
            {
                case DrawType.Indexed:
                    DrawIndexed(drawCallData.DrawMode, drawCallData.IndexedDraw.IndexCount);
                    break;
                case DrawType.Arrays:
                    DrawArrays(drawCallData.DrawMode, drawCallData.ArraysDraw.StartIndex, drawCallData.ArraysDraw.VertexCount);
                    break;
                default:
                    Debug.Error($"Draw type: '{drawCallData.DrawType}' is not implemented.");
                    break;
            }

            if (renderTarget != null)
            {
                renderTarget.Unbind();
            }

        }

        internal override byte[] ReadRenderTargetColors(GfxResource nativeResource)
        {
            if (nativeResource is GLFrameBuffer buffer)
            {
                return buffer.ReadPixels();
            }

            return null;
        }
    }
}
