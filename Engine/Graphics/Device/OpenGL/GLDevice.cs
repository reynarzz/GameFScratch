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
            glClearColor(config.Color.R, config.Color.G, config.Color.B, config.Color.A);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
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

        internal override void UpdateResouce(GfxResource resource, ResourceDescriptorBase desc)
        {
            if (resource as GLGeometry != null)
            {
                (resource as GLGeometry).UpdateResource(desc as GeometryDescriptor);
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
        private void SetUniforms(GfxResource shaderRes, UniformValue[] uniforms)
        {
            var shader = shaderRes as GLShader;
            foreach (var uniform in uniforms)
            {
                if (string.IsNullOrEmpty(uniform.Name))
                    break;

                switch (uniform.Type)
                {
                    case UniformType.Int:
                        shader.SetUniform(uniform.Name, uniform.IntValue);
                        break;
                    case UniformType.Float:
                        shader.SetUniformF(uniform.Name, uniform.FloatValue);
                        break;
                    case UniformType.Uint:
                        shader.SetUniform(uniform.Name, uniform.UIntValue);
                        break;
                    case UniformType.Mat4:
                        shader.SetUniform(uniform.Name, uniform.Mat4Value);
                        break;
                    case UniformType.Vec2:
                        shader.SetUniform(uniform.Name, uniform.Vec2Value);
                        break;
                    case UniformType.Vec3:
                        shader.SetUniform(uniform.Name, uniform.Vec3Value);
                        break;
                    case UniformType.IntArr:
                        shader.SetUniform(uniform.Name, uniform.IntArrValue);
                        break;
                    default:
                        Debug.Error($"uniform type: '{uniform.Type}' is not implemented.");
                        break;
                }
            }
        }

        private void SetPipelineFeatures(PipelineFeatures features)
        {
            if (features.Blending.Enabled)
            {
                glEnable(GL_BLEND);
                glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            }
            else
            {
                glDisable(GL_BLEND);
            }
        }

        internal override void Draw(DrawCallData drawCallData)
        {
            (drawCallData.Geometry as GLGeometry).Bind();
            var shader = drawCallData.Shader as GLShader;
            shader.Bind();

            for (int i = 0; i < drawCallData.Textures.Length; i++)
            {
                var tex = drawCallData.Textures[i];
                if (tex == null)
                    break;
                (tex as GLTexture).Bind(i);
            }

            SetUniforms(shader, drawCallData.Uniforms);
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

        }
    }
}
