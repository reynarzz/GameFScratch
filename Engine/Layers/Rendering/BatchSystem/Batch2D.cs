using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GlmNet;
using Engine.Graphics.OpenGL;

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
        internal DrawMode DrawMode { get; set; } = DrawMode.Triangles;
        internal DrawType DrawType { get; set; } = DrawType.Indexed;
        internal mat4 WorldMatrix { get; set; } = mat4.identity();
        public int SortOrder { get; set; }
        public event Action<Batch2D> OnBatchEmpty;

        private GeometryDescriptor _geoDescriptor;
        private Vertex[] _verticesData;
        public bool _isDirty;
        private Dictionary<Renderer, RendererIds> _renderers;

        private struct RendererIds
        {
            public int RendererId;
            public int TextureId;
        }

        internal Batch2D(int maxVertexSize, GfxResource sharedIndexBuffer)
        {
            MaxVertexSize = maxVertexSize;
            _verticesData = new Vertex[MaxVertexSize];
            Textures = new Texture[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits];
            _renderers = new Dictionary<Renderer, RendererIds>();
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
            vertexDesc.BufferDesc.Buffer = MemoryMarshal.AsBytes<Vertex>(_verticesData).ToArray();
            vertexDesc.BufferDesc.Usage = BufferUsage.Dynamic;
            _geoDescriptor.SharedIndexBuffer = sharedIndexBuffer;

            unsafe
            {
                vertexDesc.Attribs = new VertexAtrib[]
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

        internal void Initialize(Renderer2D renderer)
        {
            if (IsActive)
                return;

            Clear();

            SortOrder = renderer.SortOrder;
        }

        internal void Clear()
        {
            SortOrder = 0;
            VertexCount = 0;
            IndexCount = 0;
            Material = null;
            _isDirty = false;
            IsActive = false;
            _renderers.Clear();

            for (int i = 0; i < Textures.Length; i++)
            {
                Textures[i] = null;
            }
        }

        internal void PushGeometry(Renderer2D renderer, Material material, Texture texture, int indicesCount, Span<Vertex> vertices)
        {
            if (vertices.Length == 0)
                return;

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
            var existId = _renderers.TryGetValue(renderer, out var rendererIds);

            if (existId)
            {
                startIndex = rendererIds.RendererId;
            }
            else
            {
                startIndex = VertexCount;
                _renderers.Add(renderer, new RendererIds() { RendererId = startIndex, TextureId = textureIndex });
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

            _isDirty = true;
            var rendererIds = _renderers[renderer];
            _renderers.Remove(renderer);

            if (_renderers.Count == 0)
            {
                IsActive = false;
                OnBatchEmpty?.Invoke(this);
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


            bool canRemoveTexture = !_renderers.Values.Any(r => r.TextureId == rendererIds.TextureId);

            // Remove the texture if is no longer used. To save a slot.
            if (canRemoveTexture)
            {
                Debug.Log("Remove texture: " + Textures[rendererIds.TextureId].Name);
                Textures[rendererIds.TextureId] = null;
            }

            if (rendererIds.RendererId + rendererVerticesCount < _verticesData.Length)
            {
                int removedStart = rendererIds.RendererId;
                int removedCount = rendererVerticesCount;

                foreach (var kv in _renderers)
                {
                    var otherRenderer = kv.Key;
                    var otherStartids = kv.Value;

                    if (kv.Key != renderer && kv.Value.TextureId > rendererIds.TextureId)
                    {
                        var isSlotOccupied = _renderers.Any(x => x.Value.TextureId == otherStartids.TextureId - 1);

                        if (!isSlotOccupied)
                        {
                            otherStartids.TextureId--;
                            _verticesData[otherStartids.RendererId].TextureIndex = otherStartids.TextureId;
                            Debug.Log($"Change {kv.Key.Name} texture index: " + otherStartids.TextureId);
                            _renderers[kv.Key] = otherStartids;
                        }
                    }

                    if (otherStartids.RendererId > removedStart)
                    {
                        // Shift renderer ID down by the number of removed vertices
                        otherStartids.RendererId -= removedCount;
                        _renderers[otherRenderer] = otherStartids;
                    }
                }

                int startIndex = rendererIds.RendererId;
                int countToRemove = rendererVerticesCount;
                int remaining = VertexCount - (startIndex + countToRemove);

                if (canRemoveTexture)
                {
                    Array.Copy(Textures, rendererIds.TextureId + 1, Textures, rendererIds.TextureId, Textures.Length - (rendererIds.TextureId + 1));
                }

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

            // Also removes the textures from the material to avoid binding more textures than the plaform supports.
            for (int i = 0; i < Textures.Length - Material.Textures.Count; i++)
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
