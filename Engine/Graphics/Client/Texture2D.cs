using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine
{
    public class Texture2D : Texture
    {
        public TextureAtlasData Atlas { get; } = new();
        public int PixelPerUnit { get; set; } = 32;

        public Texture2D(string path, Guid guid, TextureMode mode, int width, int height, int channels, byte[] data) : 
                base(path, guid, mode, width, height, channels, data)
        {
        }

        public Texture2D(TextureMode mode, int width, int height, int channels, byte[] data) : this(string.Empty, Guid.NewGuid(),
            mode, width, height, channels, data) 
        {
        }

        protected override IResourceHandle Create()
        {
            return GfxDeviceManager.Current.CreateTexture(new TextureDescriptor()
            {
                Width = Width,
                Height = Height,
                Channels = Channels,
                Buffer = Data,
                Mode = Mode
            });
        }
    }
}
