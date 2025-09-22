using Engine.Graphics;
using GlmSharp;
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
        internal int BatchCount => _batches.Count;
        internal int IndicesToDraw { get; private set; }

        private List<Batch> _batches;

        private GfxResource _sharedIndexBuffer;

        private const int IndicesPerQuad = 6;
        private Dictionary<BucketKey, List<Renderer2D>> _renderBuckets;

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
            desc.Usage = BufferUsage.Dynamic;
            desc.Buffer = MemoryMarshal.AsBytes<uint>(indices).ToArray();

            _sharedIndexBuffer = GfxDeviceManager.Current.CreateIndexBuffer(desc);
        }

        internal IReadOnlyCollection<Batch> CreateBatches(IReadOnlyList<Renderer2D> renderers)
        {
            _renderBuckets.Clear();

            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (!renderer.IsEnabled || !renderer.Actor.IsEnabled)
                {
                    continue;
                }

                var mat = renderer.Material;

                if (!mat)
                {
                    var pinkMaterial = new Material();
                    mat = pinkMaterial;
                }

                var key = new BucketKey() { SortOrder = renderer.SortOrder, Material = renderer.Material };

                if (!_renderBuckets.ContainsKey(key))
                {
                    _renderBuckets.Add(key, new List<Renderer2D>());
                }

                _renderBuckets[key].Add(renderers[i]);
            }

            var batches = new List<Batch>();
            Log.Debug("Buckets : " + _renderBuckets.Count);

            foreach (var bucket in _renderBuckets.Values)
            {
                bucket.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

                if (bucket.Count > 0)
                {
                    // TODO: get batch from batch pool
                    var batch = new Batch();

                    // TODO: split batch if max texture unit per hardware is reached for this bucket.
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        var renderer = bucket[i];
                        if (renderer.Mesh == null)
                        {
                            var chunk = renderer.Sprite?.GetAtlasChunk() ?? AtlasChunk.DefaultChunk;
                            var worldMatrix = renderer.Transform.WorldMatrix;
                            float width = chunk.Width;
                            float height = chunk.Height;

                            if (renderer.Sprite?.Texture)
                            {
                                float ppu = renderer.Sprite.Texture.PixelPerUnit;
                                width = (float)chunk.Width / ppu;
                                height = (float)chunk.Height / ppu;
                            }

                            float px = chunk.Pivot.x * width;
                            float py = chunk.Pivot.y * height;

                            // Render quad
                            var bl = new Vertex()
                            {
                                Color = renderer.PacketColor,
                                Position = (worldMatrix * new vec4(-px, -py, 0, 1)).xyz,
                                UV = chunk.BLuv,
                            };

                            var tl = new Vertex()
                            {
                                Color = renderer.PacketColor,
                                Position = (worldMatrix * new vec4(-px, height - py, 0, 1)).xyz,
                                UV = chunk.TLuv,
                            };

                            var tr = new Vertex()
                            {
                                Color = renderer.PacketColor,
                                Position = (worldMatrix * new vec4(width - px, height - py, 0, 1)).xyz,
                                UV = chunk.TRuv,
                            };

                            var br = new Vertex()
                            {
                                Color = renderer.PacketColor,
                                Position = (worldMatrix * new vec4(width - px, -py, 0, 1)).xyz,
                                UV = chunk.BRuv,
                            };
                        }
                        else
                        {
                            // Render mesh
                        }
                    }

                    batches.Add(batch);
                }

                // TODO: create batches

                // GfxDeviceManager.Current.UpdateResouce();

            }

            // 1-Sort all by material

            // If these are the same, put int same batch
            // Material
            // Sorting order (if a)


            // 

            return batches;
        }
    }
}