using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Texture : EObject
    {
        public int Width { get; }
        public int Height { get; }
        public int Channels { get; }

        internal GfxResource NativeTexture { get; }

        internal Texture(int width, int height, int channels, byte[] data)
        {
            Width = width;
            Height = height;
            Channels = channels;
            NativeTexture = GfxDeviceManager.Current.CreateTexture(new TextureDescriptor() 
            {
                Width = width,
                Height = height,
                Channels = channels,
                Buffer = data
            });
        }
    }
}
