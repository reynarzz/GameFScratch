using Engine.Graphics;
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
        private Dictionary<BucketKey, List<SpriteRenderer>> _renderBuckets;

        // TODO: implement has code
        struct BucketKey : IEquatable<BucketKey>
        {
            public Material Material;
            public int SortOrder;

            // Required for Dictionary
            public bool Equals(BucketKey other)
            {
                return ReferenceEquals(Material, other.Material) && SortOrder == other.SortOrder;
            }

            public override bool Equals(object obj)
            {
                return obj is BucketKey other && Equals(other);
            }

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
            _renderBuckets = new Dictionary<BucketKey, List<SpriteRenderer>>();
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

        internal IReadOnlyCollection<Batch> CreateBatches(IReadOnlyList<SpriteRenderer> renderers)
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
                    _renderBuckets.Add(key, new List<SpriteRenderer>());
                }

                _renderBuckets[key].Add(renderers[i]);
            }

            var batches = new List<Batch>();
            Log.Debug("Buckets : " + _renderBuckets.Count);

            foreach (var bucket in _renderBuckets.Values)
            {
                bucket.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));


                if(bucket.Count > 0)
                {
                    var batch = new Batch();

                    for (int i = 0; i < bucket.Count; i++)
                    {
                        var renderer = bucket[i];

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