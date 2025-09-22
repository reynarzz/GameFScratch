using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal enum DrawMode 
    {
        Triangles,
        Lines,
        Points
    }

    internal struct GfxDeviceInfo 
    {
        public int MaxTexAccessInVertexShader { get; internal set; }
        public string Vendor { get; internal set; }
        public string Renderer { get; internal set; }
        public string Version { get; internal set; }
        internal string DeviceName { get; set; }
        internal int MaxTextureUnits { get; set; }
    }

    internal abstract class GfxDevice
    {
        internal abstract GfxDeviceInfo GetDeviceInfo();
        internal abstract void DrawIndexed(DrawMode mode, int indicesLength);
        internal abstract void Clear(ClearDeviceConfig config);
        internal abstract GfxResource CreateGeometry(GeometryDescriptor desc);
        internal abstract GfxResource CreateShader(ShaderDescriptor desc);
        internal abstract GfxResource CreateTexture(TextureDescriptor desc);
        internal abstract GfxResource CreateIndexBuffer(BufferDataDescriptor desc);
        internal abstract GfxResource CreateVertexBuffer(VertexDataDescriptor desc);

        internal abstract void UpdateResouce(GfxResource resource, IResourceDescriptor desc);
        internal abstract void SetPipelineFeatures(PipelineFeatures features);
    }
}