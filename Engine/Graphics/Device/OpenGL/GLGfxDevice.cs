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
    }
}
