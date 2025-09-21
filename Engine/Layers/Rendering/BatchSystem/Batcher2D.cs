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
        private Dictionary<Material, List<SpriteRenderer>> _renderBuckets;

        public Batcher2D(int maxQuadsPerBatch)
        {
            MaxQuadsPerBatch = maxQuadsPerBatch;
            _renderBuckets = new Dictionary<Material, List<SpriteRenderer>>();
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
                if(!renderers[i].IsEnabled || !renderers[i].Actor.IsEnabled)
                {
                    continue;
                }

                var mat = renderers[i].Material;

                if (!mat)
                {
                    var pinkMaterial = new Material();
                    mat = pinkMaterial;
                }

                if(_renderBuckets[mat] == null)
                {
                    _renderBuckets[mat] = new List<SpriteRenderer>();
                }

                _renderBuckets[mat].Add(renderers[i]);
            }

            foreach (var bucket in _renderBuckets.Values)
            {

            }

            // 1-Sort all by material

            // If these are the same, put int same batch
            // Material
            // Sorting order (if a)


            // 

            return Array.Empty<Batch>();
        }
    }
}