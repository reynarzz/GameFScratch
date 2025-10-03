using Engine.Layers;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal abstract class GfxDevice
    {
        internal abstract void Initialize();
        internal abstract void Close();

        internal abstract GfxDeviceInfo GetDeviceInfo();
        internal abstract void DrawArrays(DrawMode mode, int startIndex, int vertexCount); // Remove this, use drawCall api
        internal abstract void Clear(ClearDeviceConfig config);
        internal abstract GfxResource CreateGeometry(GeometryDescriptor desc);
        internal abstract GfxResource CreateShader(ShaderDescriptor desc);
        internal abstract GfxResource CreateTexture(TextureDescriptor desc);
        internal abstract GfxResource CreateIndexBuffer(BufferDataDescriptor desc);
        internal abstract GfxResource CreateVertexBuffer(VertexDataDescriptor desc);
        internal abstract GfxResource CreateRenderTarget(RenderTargetDescriptor desc);

        internal abstract void UpdateGeometry(GfxResource resource, GeometryDescriptor desc);
        internal abstract void SetViewport(vec4 viewport);

        internal abstract void UpdateResouce(GfxResource resource, IGfxResourceDescriptor desc);
        internal abstract void Present(GfxResource renderTarget);
        internal abstract void Present();

        internal abstract void Draw(DrawCallData data);

        internal abstract byte[] ReadRenderTargetColors(GfxResource nativeResource);
    }
}