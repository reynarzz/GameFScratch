using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    // Note: This is here just for showcase purposes, for this particular game it will not be implemented.
    internal class D3D12Device : GfxDevice
    {
        internal override void Initialize()
        {
            throw new NotImplementedException();
        }

        internal override void Clear(ClearDeviceConfig config)
        {
            throw new NotImplementedException();
        }

        internal override void Close()
        {
            throw new NotImplementedException();
        }

        internal override GfxResource CreateGeometry(GeometryDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override GfxResource CreateIndexBuffer(BufferDataDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override GfxResource CreateShader(ShaderDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override GfxResource CreateTexture(TextureDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override GfxResource CreateVertexBuffer(VertexDataDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(DrawCallData data)
        {
            throw new NotImplementedException();
        }

        internal override void DrawArrays(DrawMode mode, int startIndex, int vertexCount)
        {
            throw new NotImplementedException();
        }

        internal override GfxDeviceInfo GetDeviceInfo()
        {
            throw new NotImplementedException();
        }

        internal override void Present()
        {
            throw new NotImplementedException();
        }

        internal override void SetViewport(vec4 viewport)
        {
            throw new NotImplementedException();
        }

        internal override void UpdateGeometry(GfxResource resource, GeometryDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override void UpdateResouce(GfxResource resource, IGfxResourceDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override GfxResource CreateRenderTarget(RenderTargetDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal override void Present(GfxResource renderTarget)
        {
            throw new NotImplementedException();
        }

        internal override byte[] ReadRenderTargetColors(GfxResource nativeResource)
        {
            throw new NotImplementedException();
        }
    }
}
