using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class FontTextureManager : ITexture2DManager
    {
        private TextureDescriptor _sharedDescriptor;
        public FontTextureManager()
        {
            _sharedDescriptor = new TextureDescriptor();
        }

        public object CreateTexture(int width, int height)
        {
            return new Texture2D(TextureMode.Clamp, width, height, 4, null);
        }

        public Point GetTextureSize(object texture)
        {
            var tex2D = texture as Texture2D;
            return new Point(tex2D.Width, tex2D.Height);
        }

        public void SetTextureData(object texture, Rectangle bounds, byte[] data)
        {
            var tex2D = texture as Texture2D;
            _sharedDescriptor.XOffset = bounds.Left;
            _sharedDescriptor.YOffset = bounds.Top;
            _sharedDescriptor.Width = bounds.Width;
            _sharedDescriptor.Height = bounds.Height;
            _sharedDescriptor.Buffer = data;

            GfxDeviceManager.Current.UpdateResouce(tex2D.NativeResource, _sharedDescriptor);
        }
    }
}
