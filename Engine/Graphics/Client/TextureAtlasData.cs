using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct QuadUV
    {
        // Uv's for quad vertices.
        public vec2 BottomLeftUV { get; set; }
        public vec2 TopLeftUV { get; set; }
        public vec2 TopRightUV { get; set; }
        public vec2 BottomRightUV { get; set; }

    }

    public struct AtlasChunk 
    {
        public static AtlasChunk DefaultChunk = new AtlasChunk()
        {
            Pivot = new vec2(0.5f, 0.5f),
            Uvs = new QuadUV()
            {
                BottomLeftUV = new vec2(0, 0),
                TopLeftUV = new vec2(0, 1),
                TopRightUV = new vec2(1, 1),
                BottomRightUV = new vec2(1, 0)
            },
            Width = 1,
            Height = 1,
        };

        public vec2 Pivot { get; set; }

        public QuadUV Uvs { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class TextureAtlasData
    {
        private AtlasChunk[] _chunks;
        public int ChunksCount => _chunks.Length;

        public TextureAtlasData()
        {
            var defaultChunk = AtlasChunk.DefaultChunk;
            defaultChunk.Width = 1;
            defaultChunk.Height = 1;

            _chunks = new AtlasChunk[1]
            {
                defaultChunk
            };
        }

        public bool HasValidChunk(int chunkIndex)
        {
            return _chunks != null && _chunks.Length > chunkIndex;
        }

        public AtlasChunk GetChunk(int index) 
        {
            if (_chunks == null) 
            {
                return AtlasChunk.DefaultChunk;
            }

            var isInvalidIndex = index >= _chunks.Length;
#if DEBUG
            if (isInvalidIndex)
            {
                Debug.Error($"invalid atlas chunk index: '{index}', Atlas Max: '{_chunks.Length}'");

                return AtlasChunk.DefaultChunk;
            }
#endif
            return _chunks[index];
        }

        public void UpdateChunk(int index, AtlasChunk chunk)
        {
            _chunks[index] = chunk;
        }
    }
}