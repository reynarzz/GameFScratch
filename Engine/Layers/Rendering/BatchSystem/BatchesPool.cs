using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal Batch2D Get(int maxVertexSize, GfxResource indexBuffer = null)
        {
            foreach (var batch in _batches)
            {
                if (batch.IsFlushed && batch.MaxVertexSize >= maxVertexSize) 
                {
                    batch.Initialize();
                    return batch;
                }
            }

            // TODO: find min that can fit this maxVertexSize, and has this indexBuffer, if no available, create one.
            //GfxDeviceManager.Current.CreateIndexBuffer();
            var newBatch = new Batch2D(maxVertexSize, _sharedIndexBuffer);

            // Initialize to clear any old states.
            newBatch.Initialize();
            Debug.Info("Create new batch");
            _batches.Add(newBatch);

            return newBatch;
        }

        internal void ClearPool() 
        {
            // Delete all elements
        }

        internal IEnumerable<Batch2D> GetActiveBatches()
        {
            return _batches.Where(x => !x.IsFlushed);
        }
    }
}