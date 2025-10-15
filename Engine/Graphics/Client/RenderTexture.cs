using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine
{
    public class RenderTexture : Texture
    {
        public RenderTexture(int width, int height) : 
            base(string.Empty, Guid.NewGuid(), width, height, 4, default(byte[]))
        {
        }

        internal RenderTexture(GfxResource renderTarget, int width, int height) : 
            base(string.Empty, Guid.NewGuid(), width, height, 4, renderTarget)
        {
        }

        public void UpdateTarget(int width, int height)
        {
            Width = width;
            Height = height;
            GfxDeviceManager.Current.UpdateResouce(NativeResource, new RenderTargetDescriptor() { Width = width, Height = height });
        }

        protected override IResourceHandle Create()
        {
           return GfxDeviceManager.Current.CreateRenderTarget(new RenderTargetDescriptor() { Width = Width, Height = Height });
        }

        public byte[] ReadColorsRGBA()
        {
            return GfxDeviceManager.Current.ReadRenderTargetColors(NativeResource);
        }
    }
}
