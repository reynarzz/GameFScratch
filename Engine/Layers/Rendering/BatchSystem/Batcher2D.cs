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
        private const int VerticesPerQuad = 4;
        private Dictionary<BucketKey, List<Renderer2D>> _renderBuckets;
        private BatchesPool _batchesPool;
        private Material _pinkMaterial;
        private Texture2D _whiteTexture;
        private readonly Vertex[] _quadVertexArray = new Vertex[4];

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
            _whiteTexture = new Texture2D(TextureMode.Clamp, 1, 1, 4, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            _whiteTexture.PixelPerUnit = 1;

            Initialize();
        }

        internal void Initialize()
        {
            _sharedIndexBuffer = GraphicsHelper.CreateQuadIndexBuffer(MaxQuadsPerBatch);
            _batchesPool = new BatchesPool(_sharedIndexBuffer);
        }


        internal IReadOnlyList<Batch2D> GetBatches(List<Renderer2D> renderers)
        {
            // TODO: Do frustum culling

            _renderBuckets.Clear();

            foreach (var renderer in renderers)
            {
                if (!renderer.IsEnabled || !renderer.Actor.IsActiveInHierarchy)
                {
                    // TODO: notify if need to be removed from a batch
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
                    var texture = renderer.Sprite?.Texture ?? _whiteTexture;
                    var material = renderer.Material ?? _pinkMaterial;

                    if (!renderer.IsDirty && !renderer.Transform.NeedsInterpolation)
                    {
                        continue;
                    }
                    else
                    {
                        renderer.MarkNotDirty();
                    }

                    if (renderer is ParticleSystem2D particle)
                    {
                        particle.Render();
                    }

                    if (renderer.Mesh == null)
                    {
                        var chunk = renderer.Sprite?.GetAtlasChunk() ?? AtlasChunk.DefaultChunk;
                        var worldMatrix = renderer.Transform.GetRenderingWorldMatrix();

                        float ppu = texture.PixelPerUnit;
                        var width = (float)chunk.Width / ppu;
                        var height = (float)chunk.Height / ppu;

                        if (!CanPushGeometry(currentBatch, VerticesPerQuad, texture, material))
                        {
                            currentBatch = _batchesPool.Get(renderer, VerticesPerQuad, MaxBatchVertexSize, material);
                        }

                        QuadVertices quad = default;
                        GraphicsHelper.CreateQuad(ref quad, chunk.Uvs, width, height, chunk.Pivot, renderer.Color, worldMatrix);

                        _quadVertexArray[0] = quad.v0;
                        _quadVertexArray[1] = quad.v1;
                        _quadVertexArray[2] = quad.v2;
                        _quadVertexArray[3] = quad.v3;

                        currentBatch.PushGeometry(renderer, material, texture, IndicesPerQuad, _quadVertexArray);
                    }
                    else
                    {
                        // TODO: implement proper mesh drawing, for now, since it is used just for tilemap, this works
                        var vertexCount = Math.Max(MaxBatchVertexSize, renderer.Mesh.Vertices.Count);

                        if (!CanPushGeometry(currentBatch, vertexCount, texture, material))
                        {
                            currentBatch = _batchesPool.Get(renderer, renderer.Mesh.Vertices.Count, vertexCount, material, GraphicsHelper.CreateQuadIndexBuffer(vertexCount / VerticesPerQuad));
                        }

                        currentBatch.PushGeometry(renderer, material, texture, renderer.Mesh.IndicesToDrawCount, CollectionsMarshal.AsSpan(renderer.Mesh.Vertices));
                    }
                }
            }

            return _batchesPool.GetActiveBatches();
        }

        private bool CanPushGeometry(Batch2D currentBatch, int vertexCount, Texture texture, Material material)
        {
            return currentBatch != null && currentBatch.CanPushGeometry(vertexCount, texture, material);
        }
    }
}