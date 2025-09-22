using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Engine.Rendering
{
    internal class Batch2D : IDisposable
    {
        public int MaxVertexSize { get; }

        internal Material Material { get; private set; }
        internal GfxResource Geometry { get; }
        internal Texture[] Textures { get; }

        internal int VertexCount { get; private set; }
        internal int IndexCount { get; private set; }
        internal bool IsFlushed { get; private set; } = false;

        private GeometryDescriptor _geoDescriptor;
        private Vertex[] _verticesData;

        internal Batch2D(int maxVertexSize, GfxResource sharedIndexBuffer)
        {
            MaxVertexSize = maxVertexSize;
            _verticesData = new Vertex[MaxVertexSize];
            Textures = new Texture[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits];
            _geoDescriptor = new GeometryDescriptor()
            {
                VertexDesc = new VertexDataDescriptor() { BufferDesc = new BufferDataDescriptor() }
            };

            // Create geometry buffer for this batch
            var geoDesc = new GeometryDescriptor();
            var vertexDesc = new VertexDataDescriptor();
            vertexDesc.BufferDesc = new BufferDataDescriptor();
            vertexDesc.BufferDesc.Buffer = MemoryMarshal.AsBytes<Vertex>(new Vertex[maxVertexSize]).ToArray();
            vertexDesc.BufferDesc.Usage = BufferUsage.Static;
            geoDesc.SharedIndexBuffer = sharedIndexBuffer;

            unsafe
            {
                vertexDesc.Attribs = new()
                {
                    new() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = 0 },                 // Position
                    new() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = sizeof(float) * 3 }, // UV
                    new() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = sizeof(float) * 5 }, // Normals
                    new() { Count = 1, Normalized = false, Type = GfxValueType.Uint, Stride = sizeof(Vertex), Offset = sizeof(float) * 6 },  // Color
                    new() { Count = 1, Normalized = false, Type = GfxValueType.Uint, Stride = sizeof(Vertex), Offset = sizeof(float) * 7 },  // TextureIndex
                };
                geoDesc.VertexDesc = vertexDesc;
            }

            Geometry = GfxDeviceManager.Current.CreateGeometry(geoDesc);
        }

        internal void Initialize()
        {
            IsFlushed = false;
            VertexCount = 0;
            IndexCount = 0;

            for (int i = 0; i < Textures.Length; i++)
            {
                Textures[i] = null;
            }
        }

        internal void PushGeometry(Material material, Texture texture, int indicesCount, params Vertex[] vertices)
        {
            if (!Material)
            {
                Material = material;
            }

            // Adds texture to a empty slot
            for (int i = 0; i < Textures.Length; i++)
            {
                if (Textures[i] == texture)
                {
                    break;
                }
                else if (Textures[i] == null)
                {
                    Textures[i] = texture;
                    break;
                }
            }

            // Copies vertices data
            for (int i = 0; i < vertices.Length; i++)
            {
                _verticesData[VertexCount + i] = vertices[i];
            }

            VertexCount += vertices.Length;
            IndexCount += indicesCount;

            // Log.Info($"Verts: {VertexCount}, indices: {IndexCount}");
        }

        /// <summary>
        /// Will push geometry immediatelly to gpu.
        /// </summary>
        internal void PushGeometryImmediate(Material material, Texture texture, int indicesCount, params Vertex[] vertices)
        {
            // TODO: 
        }

        internal void Flush()
        {
            var vertDataDescriptor = _geoDescriptor.VertexDesc.BufferDesc;
            vertDataDescriptor.Offset = 0;
            vertDataDescriptor.Buffer = MemoryMarshal.AsBytes<Vertex>(_verticesData).ToArray();


            GfxDeviceManager.Current.UpdateGeometry(Geometry, _geoDescriptor);

            IsFlushed = true;
        }

        internal bool CanPushGeometry(int vertexCount, Texture texture)
        {
            if (vertexCount + VertexCount > MaxVertexSize)
            {
                return false;
            }

            for (int i = 0; i < Textures.Length; i++)
            {
                if (texture == Textures[i] || Textures[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Geometry.Dispose();
        }
    }
}
