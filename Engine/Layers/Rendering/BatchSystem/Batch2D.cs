using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Rendering
{
    internal class Batch2D
    {
        public int MaxVertexSize { get; }

        internal Material Material { get; private set; }
        internal GfxResource Geometry { get; private set; }
        internal Texture[] Textures { get; }

        internal int VertexCount { get; private set; }
        internal int IndexCount { get; private set; }
        public bool Flushed { get; set; } = false;
        private bool HasMaxTextureUnitsFilled => _textureUsePointer > GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits;
        private int _textureUsePointer = 0;

        public Batch2D(int maxVertexSize)
        {
            MaxVertexSize = maxVertexSize;
            Textures = new Texture[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits];
        }

        public bool PushGeometry(Material material, Texture texture)
        {
            // If fits in terms of vertices and has at least a texture slot available for the sprite
            var canAddGeometry = true;
            return canAddGeometry;
        }

        public void Initialize()
        {
            Flushed = false;
            VertexCount = 0;
            _textureUsePointer = 0;
        }

        public void Flush()
        {
            Flushed = true;
        }

        internal bool CanPushGeometry(Material material, Texture texture)
        {
            if (!HasMaxTextureUnitsFilled)
            {
                return Material == material;
            }

            for (int i = 0; i < Textures.Length; i++)
            {
                return Textures[i] == texture;
            }

            return false;
        }
    }
}
