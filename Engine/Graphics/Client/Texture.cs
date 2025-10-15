using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class Texture : EObject
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int Channels { get; }
        internal byte[] Data { get; }

        internal GfxResource NativeResource { get; }

        internal Texture(string name, Guid guid, int width, int height, int channels, byte[] data) : base(name, guid)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Data = data;

            NativeResource = Create() as GfxResource;
        }

        internal Texture(string name, Guid guid, int width, int height, int channels, GfxResource nativeResource) : base(name, guid)
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
