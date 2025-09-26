using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine
{
    internal class GraphicsHelper
    {
        internal static GfxResource GetEmptyGeometry<T>(int vertCount, int indexCount, ref GeometryDescriptor geoDesc) where T: struct
        {
            geoDesc = new GeometryDescriptor();

            if (indexCount <= 0)
            {
                geoDesc.IndexDesc = null;
            }
            else
            {
                geoDesc.IndexDesc = new BufferDataDescriptor();
                geoDesc.IndexDesc.Usage = BufferUsage.Dynamic;
                geoDesc.IndexDesc.Buffer = new byte[indexCount];
            }

            geoDesc.VertexDesc = new VertexDataDescriptor();
            unsafe
            {
                geoDesc.VertexDesc.Attribs = new List<VertexAtrib>()
                {
                    new VertexAtrib() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(T), Offset = 0 }, // Position
                    new VertexAtrib() { Count = 1, Normalized = false, Type = GfxValueType.Uint, Stride = sizeof(T), Offset = sizeof(float) * 3 }, // Color
                };
            }
            geoDesc.VertexDesc.BufferDesc = new BufferDataDescriptor();
            geoDesc.VertexDesc.BufferDesc.Buffer = new byte[vertCount];
            geoDesc.VertexDesc.BufferDesc.Usage = BufferUsage.Dynamic;

            return GfxDeviceManager.Current.CreateGeometry(geoDesc);
        }
    }
}
