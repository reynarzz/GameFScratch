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

        internal Batch2D Get(Renderer renderer, int vertexToAdd, int maxVertexSize, Material mat, GfxResource indexBuffer = null)
        {
            foreach (var batch in _batches)
            {
                var isTotalSizeEnough = batch.MaxVertexSize >= maxVertexSize;
                var hasSpaceLeftForAnother = batch.VertexCount > vertexToAdd && !batch.Contains(renderer);
                var alreadyHasRenderer = batch.Contains(renderer);

                if (isTotalSizeEnough && (hasSpaceLeftForAnother || alreadyHasRenderer) && batch.Material == mat)
                {
                    batch.Initialize();
                    return batch;
                }
            }

            // TODO: find min that can fit this maxVertexSize, and has this indexBuffer, if no available, create one.
            //GfxDeviceManager.Current.CreateIndexBuffer();

            
            Batch2D newBatch = new Batch2D(maxVertexSize, indexBuffer == null ? _sharedIndexBuffer : indexBuffer);
            newBatch.OnBatchEmpty += OnBatchEmpty;
            // Initialize to clear any old states.
            newBatch.Initialize();
            Debug.Info("Create new batch");

            _batches.Add(newBatch);

            return newBatch;
        }

        private void OnBatchEmpty(Batch2D batch)
        {
            batch.IsActive = false;

            var index = _batches.IndexOf(batch);
            _batches.RemoveAt(index);
            bool inserted = false;
            for (int i = 0; i < _batches.Count; i++)
            {
                if (!_batches[i].IsActive)
                {
                    // Put this to the end of the the active batches
                    _batches.Insert(i, batch);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                _batches.Add(batch);
            }

            Debug.Log("pooled batch");
        }

        internal void ClearPool()
        {
            // Delete all batches that are not being used for too long, and are also big.
        }



        internal IReadOnlyList<Batch2D> GetActiveBatches()
        {
            // TODO: Make sure that all alive batches are consecutive
            return _batches;//.Where(x => x.IsActive);
        }
    }
}