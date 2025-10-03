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
        public RenderTexture(int width, int height) : base(width, height, 4, null)
        {
        }

        public void UpdateTarget(int width, int height)
        {
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
