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
        internal bool IsActive { get; private set; }
        public mat4 WorldMatrix = mat4.identity();

        // TODO: On batch empty just
        public event Action<Batch2D> OnBatchEmpty;

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

        internal void PushGeometry(Renderer renderer, Material material, Texture texture, int indicesCount, Span<Vertex> vertices)
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

            renderer.OnDestroyRenderer -= OnRendererDestroy;
            renderer.OnDestroyRenderer += OnRendererDestroy;

            var startIndex = 0;
            var existId = _renderers.ContainsKey(renderer);

            if (existId)
            {
                startIndex = renderer.RendererID;
            }
            else
            {
                startIndex = VertexCount;
                renderer.RendererID = startIndex;
                _renderers.Add(renderer, startIndex);
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
        }

        /// <summary>
        /// Will push geometry immediatelly to gpu.
        /// </summary>
        internal void PushGeometryImmediate(Material material, Texture texture, int indicesCount, params Vertex[] vertices)
        {
            // TODO: 
        }

        private void OnRendererDestroy(Renderer renderer)
        {
            renderer.OnDestroyRenderer -= OnRendererDestroy;

            _renderers.Remove(renderer);
            
            if (_renderers.Count == 0)
            {
                OnBatchEmpty?.Invoke(this);
                IsActive = false;
                return;
            }

            var rendererVerticesCount = 0;
            var rendererIndicesCount = 0;

            if (renderer.Mesh == null)
            {
                // This is rendering quads, and no a custom mesh
                rendererVerticesCount = 4;
                rendererIndicesCount = 6;
            }
            else
            {
                rendererVerticesCount = renderer.Mesh.Vertices.Count;
                rendererIndicesCount = renderer.Mesh.IndicesToDrawCount;
            }

            if (renderer.RendererID + rendererVerticesCount < _verticesData.Length)
            {
                int startIndex = renderer.RendererID;
                int countToRemove = rendererVerticesCount;

                int remaining = VertexCount - (startIndex + countToRemove);

                // Shift the trailing vertices down
                Array.Copy(_verticesData,
                           startIndex + countToRemove,
                           _verticesData,
                           startIndex,
                           remaining);
            }

            // Decrease the amount of vertices and indices to draw
            VertexCount -= rendererVerticesCount;
            IndexCount -= rendererIndicesCount;

            bool canRemoveTexture = !_renderers.Keys.Any(r => r.TextureRendererID == renderer.TextureRendererID);

            // Remove the texture if is no longer used. To save a slot.
            if (canRemoveTexture)
            {
                Textures[renderer.TextureRendererID] = null;
            }
        }

        internal void Flush()
        {
            if (_isDirty)
            {
                var vertDataDescriptor = _geoDescriptor.VertexDesc.BufferDesc;
                vertDataDescriptor.Offset = 0;

                unsafe
                {
                    vertDataDescriptor.Count = sizeof(Vertex) * VertexCount;
                }

                vertDataDescriptor.Buffer = MemoryMarshal.AsBytes<Vertex>(_verticesData).ToArray();

                GfxDeviceManager.Current.UpdateGeometry(Geometry, _geoDescriptor);
            }

            _isDirty = false;
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
