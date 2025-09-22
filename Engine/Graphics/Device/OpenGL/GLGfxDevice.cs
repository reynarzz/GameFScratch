using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLGfxDevice : GfxDevice
    {
        private readonly GfxDeviceInfo _gfxDeviceInfo;
        public GLGfxDevice()
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
            glClearColor(config.Color.x, config.Color.y, config.Color.z, config.Color.w);
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


        internal override void DrawIndexed(DrawMode mode, int indicesLength)
        {
            var glMode = mode switch
            {
                DrawMode.Triangles => GL_TRIANGLES,
                DrawMode.Lines => GL_LINES,
                DrawMode.Points => GL_POINTS,
                _ => 0
            };

            if(glMode == 0)
            {
                Log.Error($"Draw mode unsupported: {mode}");
                return;
            }

            unsafe
            {
                glDrawElements(glMode, indicesLength, GL_UNSIGNED_INT, null);
            }
        }

        internal override void SetPipelineFeatures(PipelineFeatures features)
        {
            if (features.Blending.Enabled)
            {
                glEnable(GL_BLEND);
            }
            else 
            {
                glDisable(GL_BLEND);
            }
        }

        internal override void UpdateResouce(GfxResource resource, IResourceDescriptor desc)
        {
            if(resource as GLGeometry != null)
            {
                (resource as GLGeometry).UpdateResource(desc as GeometryDescriptor);
            }
        }

        internal override GfxDeviceInfo GetDeviceInfo()
        {
            return _gfxDeviceInfo;
        }
    }
}
