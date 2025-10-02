using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Rendering
{
    internal class BatchesPool
    {
        private readonly GfxResource _sharedIndexBuffer;
        private List<Batch2D> _batches;

        public BatchesPool(GfxResource sharedIndexBuffer)
        {
            _sharedIndexBuffer = sharedIndexBuffer;
            _batches = new List<Batch2D>();
        }

        internal Batch2D Get(Renderer2D renderer, int vertexToAdd, int maxVertexSize, Material mat, GfxResource indexBuffer = null)
        {
            foreach (var batch in _batches)
            {
                if (batch.Contains(renderer))
                {
                    return batch;
                }
            }

            foreach (var batch in _batches)
            {
                var isTotalSizeEnough = batch.MaxVertexSize >= maxVertexSize;
                var hasSpaceLeftForAnother = (batch.MaxVertexSize - batch.VertexCount) > vertexToAdd;
                //var hasSpaceLeftForAnother = batch.VertexCount > vertexToAdd && !batch.Contains(renderer);

                if (isTotalSizeEnough && hasSpaceLeftForAnother && (batch.Material == mat || batch.Material == null) && (renderer.SortOrder == batch.SortOrder || !batch.IsActive))
                {
                    batch.Initialize(renderer);
                    _batches.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));

                    return batch;
                }
            }

            Batch2D newBatch = new Batch2D(maxVertexSize, indexBuffer == null ? _sharedIndexBuffer : indexBuffer);
            newBatch.OnBatchEmpty += OnBatchEmpty;
            // Initialize to clear any old states.
            newBatch.Initialize(renderer);
            Debug.Info("Create new batch");

            _batches.Add(newBatch);
            _batches.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));

            return newBatch;
        }

        private void OnBatchEmpty(Batch2D batch)
        {
            // Moves empty batch to the end.
            _batches.Remove(batch);
            _batches.Add(batch);
            batch.Clear();

            Debug.Log("pooled batch");
        }

        internal void ClearPool()
        {
            // Delete all batches that are not being used for too long, and are also big.
        }

        internal IReadOnlyList<Batch2D> GetActiveBatches()
        {
            return _batches;
        }
    }
}