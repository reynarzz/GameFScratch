using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using GlmNet;

namespace Engine
{
    internal class GraphicsHelper
    {
        internal static GfxResource GetEmptyGeometry<T>(int vertCount, int indexCount, ref GeometryDescriptor geoDesc) where T : struct
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

        internal static GfxResource GetScreenQuadGeometry()
        {
            var geoDesc = new GeometryDescriptor();

            geoDesc.IndexDesc = new BufferDataDescriptor();
            geoDesc.IndexDesc.Usage = BufferUsage.Static;
            geoDesc.IndexDesc.Buffer = MemoryMarshal.AsBytes([0, 1, 2, 0, 2, 3]).ToArray();

            geoDesc.VertexDesc = new VertexDataDescriptor();

            unsafe
            {
                geoDesc.VertexDesc.Attribs = new List<VertexAtrib>()
                {
                    new VertexAtrib() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = 0 }, // Position
                    new VertexAtrib() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = sizeof(float) * 3 }, // UV
                };
            }

            QuadVertices vertices = default;
            CreateQuad(ref vertices, QuadUV.DefaultUVs, 2, 2, new vec2(0.5f), Color.White, mat4.identity());

            geoDesc.VertexDesc.BufferDesc = new BufferDataDescriptor();
            geoDesc.VertexDesc.BufferDesc.Buffer = MemoryMarshal.AsBytes([vertices.v0, vertices.v1, vertices.v2, vertices.v3]).ToArray();
            geoDesc.VertexDesc.BufferDesc.Usage = BufferUsage.Static;

            return GfxDeviceManager.Current.CreateGeometry(geoDesc);
        }

        internal static void CreateQuad(ref QuadVertices vertices, QuadUV uvs, float width, float height, vec2 pivot,
                                         ColorPacketRGBA color, mat4 worldMatrix)
        {
            float px = pivot.x * width;
            float py = pivot.y * height;

            vertices.v0 = new Vertex()
            {
                Color = color,
                Position = new vec3(worldMatrix * new vec4(-px, -py, 0, 1)),
                UV = uvs.BottomLeftUV,
            };

            vertices.v1 = new Vertex()
            {
                Color = color,
                Position = new vec3(worldMatrix * new vec4(-px, height - py, 0, 1)),
                UV = uvs.TopLeftUV,
            };

            vertices.v2 = new Vertex()
            {
                Color = color,
                Position = new vec3(worldMatrix * new vec4(width - px, height - py, 0, 1)),
                UV = uvs.TopRightUV,
            };

            vertices.v3 = new Vertex()
            {
                Color = color,
                Position = new vec3(worldMatrix * new vec4(width - px, -py, 0, 1)),
                UV = uvs.BottomRightUV
            };
        }
    }
}
