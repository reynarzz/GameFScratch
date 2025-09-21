using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public enum DrawMode 
    {
        Triangles,
        Lines,
        Points
    }

    internal abstract class GfxDevice
    {
        internal abstract void DrawIndexed(DrawMode mode, int indicesLength);
        internal abstract void Clear(ClearDeviceConfig config);
        internal abstract GfxResource CreateGeometry(GeometryDescriptor desc);
        internal abstract GfxResource CreateShader(ShaderDescriptor desc);
        internal abstract GfxResource CreateTexture(TextureDescriptor desc);
        internal abstract void UpdateResouce(GfxResource resource, IResourceDescriptor desc);
        internal abstract void SetPipelineFeatures(PipelineFeatures features);
    }
}