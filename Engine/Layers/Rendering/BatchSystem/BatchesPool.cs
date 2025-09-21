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
        public Batch Get(int maxVertexSize, GfxResource indexBuffer)
        {
            // TODO: find min that can fit this maxVertexSize, and has this indexBuffer, if no available, create one.
            //GfxDeviceManager.Current.CreateIndexBuffer();

            return new Batch();
        }

        public void ClearPool() 
        {
            // Delete all elements
        }
    }
}