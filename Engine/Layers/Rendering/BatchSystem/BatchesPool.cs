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
        internal Batch2D Get(int maxVertexSize, Material material, Texture texture, GfxResource indexBuffer = null)
        {
            // TODO: find min that can fit this maxVertexSize, and has this indexBuffer, if no available, create one.
            //GfxDeviceManager.Current.CreateIndexBuffer();
            var batch = new Batch2D(maxVertexSize);

            // Initialize to clear any old states.
            batch.Initialize();

            return batch;
        }

        internal void ClearPool() 
        {
            // Delete all elements
        }

        internal IReadOnlyCollection<Batch2D> GetActiveBatches()
        {
            // TODO:
            return new List<Batch2D>();
        }
    }
}