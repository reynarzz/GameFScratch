using Engine.Graphics;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Rendering
{
    internal class Batcher2D
    {
        internal int MaxQuadsPerBatch { get; private set; }
        private int MaxBatchVertexSize => MaxQuadsPerBatch * 4;

        internal int BatchCount => _batches.Count;
        internal int IndicesToDraw { get; private set; }

        private List<Batch2D> _batches;

        private GfxResource _sharedIndexBuffer;

        private const int IndicesPerQuad = 6;
        private Dictionary<BucketKey, List<Renderer2D>> _renderBuckets;
        private BatchesPool _batchesPool;
        private Material _pinkMaterial;
        private Texture2D _whiteTexture;

        private struct BucketKey : IEquatable<BucketKey>
        {
            public Material Material { get; set; }
            public int SortOrder { get; set; }

            public bool Equals(BucketKey other) => ReferenceEquals(Material, other.Material) && SortOrder == other.SortOrder;
            public override bool Equals(object obj) => obj is BucketKey other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + (Material != null ? Material.GetHashCode() : 0);
                    hash = hash * 31 + SortOrder;
                    return hash;
                }
            }

            public static bool operator ==(BucketKey a, BucketKey b) => a.Equals(b);
            public static bool operator !=(BucketKey a, BucketKey b) => !a.Equals(b);
        }

        public Batcher2D(int maxQuadsPerBatch)
        {
            MaxQuadsPerBatch = maxQuadsPerBatch;
            _renderBuckets = new Dictionary<BucketKey, List<Renderer2D>>();

            _pinkMaterial = new Material(Tests.GetShaderPink());
            _whiteTexture = new Texture2D(1, 1, 4, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            _whiteTexture.PixelPerUnit = 1;

            Initialize();
        }

        internal void Initialize()
        {
            var indices = new uint[MaxQuadsPerBatch * IndicesPerQuad];

            for (uint i = 0; i < MaxQuadsPerBatch; i++)
            {
                indices[i * 6 + 0] = i * 4 + 0;
                indices[i * 6 + 1] = i * 4 + 1;
                indices[i * 6 + 2] = i * 4 + 2;
                indices[i * 6 + 3] = i * 4 + 2;
                indices[i * 6 + 4] = i * 4 + 3;
                indices[i * 6 + 5] = i * 4 + 0;
            }

            var desc = new BufferDataDescriptor();
            desc.Usage = BufferUsage.Static;
            desc.Buffer = MemoryMarshal.AsBytes<uint>(indices).ToArray();

            _sharedIndexBuffer = GfxDeviceManager.Current.CreateIndexBuffer(desc);
            _batchesPool = new BatchesPool(_sharedIndexBuffer);
        }

        internal IEnumerable<Batch2D> CreateBatches(List<Renderer2D> renderers)
        {
            // TODO: Do frustum culling

            _renderBuckets.Clear();

            foreach (var renderer in renderers)
            {
                if (!renderer.IsEnabled || !renderer.Actor.IsEnabled)
                {
                    continue;
                }

                var key = new BucketKey() { SortOrder = renderer.SortOrder, Material = renderer.Material };

                if (!_renderBuckets.ContainsKey(key))
                {
                    _renderBuckets.Add(key, new List<Renderer2D>());
                }

                _renderBuckets[key].Add(renderer);
            }

            // TODO: improve performance of order by sorting, is allocating every frame
            foreach (var bucket in _renderBuckets.Values.OrderBy(x => x[0].SortOrder))
            {
                Batch2D currentBatch = null;

                foreach (var renderer in bucket)
                {
                    if (renderer.Mesh == null)
                    {
                        var chunk = renderer.Sprite?.GetAtlasChunk() ?? AtlasChunk.DefaultChunk;

                        var worldMatrix = !renderer.Transform.NeedsInterpolation ? renderer.Transform.WorldMatrix : renderer.Transform.InterpolatedWorldMatrix;
                      
                        var texture = renderer.Sprite?.Texture ?? _whiteTexture;
                        var material = renderer.Material ?? _pinkMaterial;

                        float ppu = texture.PixelPerUnit;
                        var width = (float)chunk.Width / ppu;
                        var height = (float)chunk.Height / ppu;

                        float px = chunk.Pivot.x * width;
                        float py = chunk.Pivot.y * height;

                        var v0 = new Vertex()
                        {
                            Color = renderer.PacketColor,
                            Position = new vec3(worldMatrix * new vec4(-px, -py, 0, 1)),
                            UV = chunk.BottomLeftUV,
                        };

                        var v1 = new Vertex()
                        {
                            Color = renderer.PacketColor,
                            Position = new vec3(worldMatrix * new vec4(-px, height - py, 0, 1)),
                            UV = chunk.TopLeftUV,
                        };

                        var v2 = new Vertex()
                        {
                            Color = renderer.PacketColor,
                            Position = new vec3(worldMatrix * new vec4(width - px, height - py, 0, 1)),
                            UV = chunk.TopRightUV,
                        };

                        var v3 = new Vertex()
                        {
                            Color = renderer.PacketColor,
                            Position = new vec3(worldMatrix * new vec4(width - px, -py, 0, 1)),
                            UV = chunk.BottomRightUV
                        };

                        if (currentBatch == null || !currentBatch.CanPushGeometry(4, texture))
                        {
                            currentBatch = _batchesPool.Get(MaxBatchVertexSize);
                        }

                        currentBatch.PushGeometry(material, texture, 6, v0, v1, v2, v3);
                    }
                    else
                    {
                        // TODO: Populate batch with mesh data
                    }
                }
            }

            return _batchesPool.GetActiveBatches();
        }
    }
}