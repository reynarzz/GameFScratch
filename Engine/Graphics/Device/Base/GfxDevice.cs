using GlmNet;
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
        Points,
        LineStrips
    }

    internal struct GfxDeviceInfo 
    {
        internal int MaxTexAccessInVertexShader { get; set; }
        internal int MaxHardwareTextureUnits { get; set; }
        internal int MaxValidTextureUnits { get; set; }
        internal string Vendor { get; set; }
        internal string Renderer { get; set; }
        internal string Version { get; set; }
        internal string DeviceName { get; set; }
    }

    internal abstract class GfxDevice
    {
        internal abstract void Initialize();
        internal abstract void Close();

        internal abstract GfxDeviceInfo GetDeviceInfo();
        internal abstract void DrawIndexed(DrawMode mode, int indicesLength);
        internal abstract void DrawArrays(DrawMode mode, int startIndex, int vertexCount);
        internal abstract void Clear(ClearDeviceConfig config);
        internal abstract GfxResource CreateGeometry(GeometryDescriptor desc);
        internal abstract GfxResource CreateShader(ShaderDescriptor desc);
        internal abstract GfxResource CreateTexture(TextureDescriptor desc);
        internal abstract GfxResource CreateIndexBuffer(BufferDataDescriptor desc);
        internal abstract GfxResource CreateVertexBuffer(VertexDataDescriptor desc);

        internal abstract void UpdateGeometry(GfxResource resource, GeometryDescriptor desc);
        internal abstract void SetViewport(vec4 viewport);

        internal abstract void UpdateResouce(GfxResource resource, ResourceDescriptorBase desc);
        internal abstract void SetPipelineFeatures(PipelineFeatures features);
    }
}