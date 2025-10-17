using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class Texture : AssetResourceBase
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public TextureMode Mode { get; protected set; }
        public int Channels { get; }
        internal byte[] Data { get; }

        internal GfxResource NativeResource { get; }

        internal Texture(string path, Guid guid, TextureMode mode, int width, int height, int channels, byte[] data) : base(path, guid)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Data = data;
            Mode = mode;
            NativeResource = Create() as GfxResource;
        }

        internal Texture(string path, Guid guid, int width, int height, int channels, GfxResource nativeResource) : base(path, guid)
        {
            Width = width;
            Height = height;
            Channels = channels;

            NativeResource = nativeResource;
        }

        public override void OnDestroy()
        {
            NativeResource.Dispose();
        }

        protected abstract IResourceHandle Create();
    }
}
