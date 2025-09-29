using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Engine.Graphics.OpenGL;
using GlmNet;

namespace Engine.Rendering
{
    internal class Batch2D : IDisposable
    {
        public int MaxVertexSize { get; }

        internal Material Material { get; private set; }
        internal GfxResource Geometry { get; }
        internal Texture[] Textures { get; }
        internal static int[] TextureSlotArray { get; private set; }
        internal int VertexCount { get; private set; }
        internal int IndexCount { get; private set; }
        internal bool IsFlushed { get; private set; } = false;
        internal bool IsActive { get; set; }
        public mat4 WorldMatrix = mat4.identity();

        private GeometryDescriptor _geoDescriptor;
        private Vertex[] _verticesData;
        public bool _isDirty;
        private Dictionary<Renderer, int> _renderers;
        internal Batch2D(int maxVertexSize, GfxResource sharedIndexBuffer)
        {
            MaxVertexSize = maxVertexSize;
            _verticesData = new Vertex[MaxVertexSize];
            Textures = new Texture[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits];
            _renderers = new Dictionary<Renderer, int>();
            if (TextureSlotArray == null)
            {
                TextureSlotArray = new int[Textures.Length];
                for (int i = 0; i < TextureSlotArray.Length; i++)
                {
                    TextureSlotArray[i] = i;
                }
            }

            // Create geometry buffer for this batch
            _geoDescriptor = new GeometryDescriptor();
            var vertexDesc = new VertexDataDescriptor();
            vertexDesc.BufferDesc = new BufferDataDescriptor();
            vertexDesc.BufferDesc.Buffer = MemoryMarshal.AsBytes<Vertex>(new Vertex[maxVertexSize]).ToArray();
            vertexDesc.BufferDesc.Usage = BufferUsage.Dynamic;
            _geoDescriptor.SharedIndexBuffer = sharedIndexBuffer;

            unsafe
            {
                vertexDesc.Attribs = new()
                {
                    new() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = 0 },                 // Position
                    new() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = sizeof(float) * 3 }, // UV
                    new() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = sizeof(float) * 5 }, // Normals
                    new() { Count = 1, Normalized = false, Type = GfxValueType.Uint,  Stride = sizeof(Vertex), Offset = sizeof(uint)  * 8 },  // Color
                    new() { Count = 1, Normalized = false, Type = GfxValueType.Int,   Stride = sizeof(Vertex), Offset = sizeof(int)   * 9 },  // TextureIndex
                };
                _geoDescriptor.VertexDesc = vertexDesc;
            }

            Geometry = GfxDeviceManager.Current.CreateGeometry(_geoDescriptor);
        }

        internal void Initialize()
        {
            if (IsActive)
                return;

            IsFlushed = false;
            VertexCount = 0;
            IndexCount = 0;
            Material = null;
            _isDirty = false;
            _renderers.Clear();

            for (int i = 0; i < Textures.Length; i++)
            {
                if (Textures[i] != null)
                {
                    // TODO: refactor here, dirty workaround for faster prototyping.
                    (Textures[i].NativeTexture as GLTexture).Unbind();
                }

                Textures[i] = null;
            }
        }

        internal void PushGeometry(Renderer rendererId, Material material, Texture texture, int indicesCount, Span<Vertex> vertices)
        {
            _isDirty = true;
            IsActive = true;
            if (!Material)
            {
                Material = material;
            }

            int textureIndex = 0;
            // Adds texture to a empty slot
            for (int i = 0; i < Textures.Length; i++)
            {
                if (Textures[i] == texture)
                {
                    textureIndex = i;
                    break;
                }
                else if (Textures[i] == null)
                {
                    Textures[i] = texture;
                    textureIndex = i;
                    break;
                }
            }

            var startIndex = VertexCount;
            var existId = _renderers.ContainsKey(rendererId);

            if (existId)
            {
                startIndex = rendererId.RendererID;
            }
            else
            {
                _renderers.Add(rendererId, VertexCount);
            }

            // Copies vertices data
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].TextureIndex = textureIndex;
                _verticesData[startIndex + i] = vertices[i];
            }

            if (!existId)
            {
                VertexCount += vertices.Length;
                IndexCount += indicesCount;
            }

            rendererId.RendererID = startIndex;
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
            if (_isDirty)
            {
                var vertDataDescriptor = _geoDescriptor.VertexDesc.BufferDesc;
                vertDataDescriptor.Offset = 0;
                vertDataDescriptor.Buffer = MemoryMarshal.AsBytes<Vertex>(_verticesData).ToArray();

                GfxDeviceManager.Current.UpdateGeometry(Geometry, _geoDescriptor);
            }

            _isDirty = false;
            IsFlushed = true;
        }

        internal bool CanPushGeometry(int vertexCount, Texture texture, Material mat)
        {
            if (vertexCount + VertexCount > MaxVertexSize)
            {
                return false;
            }

            if (mat != Material)
                return false;

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

        internal bool Contains(Renderer renderer)
        {
            return _renderers.ContainsKey(renderer);
        }
    }
}
