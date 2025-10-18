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
    internal static class GraphicsHelper
    {
        internal static GfxResource GetEmptyGeometry<T>(int vertCount, int indexCount, ref GeometryDescriptor geoDesc, VertexAtrib[] vertexAttribs, GfxResource indexBuffer = null)
        {
            if(geoDesc == null)
            {
                geoDesc = new GeometryDescriptor();
            }

            if (indexCount <= 0)
            {
                geoDesc.IndexDesc = null;
            }
            else
            {
                geoDesc.IndexDesc = new BufferDataDescriptor<uint>();
                geoDesc.IndexDesc.Usage = BufferUsage.Dynamic;
                geoDesc.IndexDesc.Buffer = new uint[indexCount];
            }

            geoDesc.SharedIndexBuffer = indexBuffer;

            geoDesc.VertexDesc = new VertexDataDescriptor();
            geoDesc.VertexDesc.Attribs = vertexAttribs;
            geoDesc.VertexDesc.BufferDesc = new BufferDataDescriptor<T>() { Buffer = new T[vertCount] };
            geoDesc.VertexDesc.BufferDesc.Usage = BufferUsage.Dynamic;

            return GfxDeviceManager.Current.CreateGeometry(geoDesc);
        }

        internal static GfxResource GetScreenQuadGeometry()
        {
            var geoDesc = new GeometryDescriptor();

            geoDesc.IndexDesc = new BufferDataDescriptor<uint>() { Buffer = [0, 1, 2, 0, 2, 3] };
            geoDesc.IndexDesc.Usage = BufferUsage.Static;

            geoDesc.VertexDesc = new VertexDataDescriptor();

            unsafe
            {
                geoDesc.VertexDesc.Attribs =
                [
                    new VertexAtrib() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = 0 }, // Position
                    new VertexAtrib() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = sizeof(Vertex), Offset = sizeof(float) * 3 }, // UV
                ];
            }

            QuadVertices vertices = default;
            CreateQuad(ref vertices, QuadUV.DefaultUVs, 2, 2, new vec2(0.5f), Color.White, mat4.identity());

            geoDesc.VertexDesc.BufferDesc = new BufferDataDescriptor<Vertex>() { Buffer = [vertices.v0, vertices.v1, vertices.v2, vertices.v3] };
            geoDesc.VertexDesc.BufferDesc.Usage = BufferUsage.Static;

            return GfxDeviceManager.Current.CreateGeometry(geoDesc);
        }

        internal static GfxResource CreateQuadIndexBuffer(int maxQuads)
        {
            var indices = new uint[maxQuads * 6];

            for (uint i = 0; i < maxQuads; i++)
            {
                indices[i * 6 + 0] = i * 4 + 0;
                indices[i * 6 + 1] = i * 4 + 1;
                indices[i * 6 + 2] = i * 4 + 2;
                indices[i * 6 + 3] = i * 4 + 2;
                indices[i * 6 + 4] = i * 4 + 3;
                indices[i * 6 + 5] = i * 4 + 0;
            }

            var desc = new BufferDataDescriptor<byte>();
            desc.Usage = BufferUsage.Static;
            desc.Buffer = MemoryMarshal.AsBytes<uint>(indices).ToArray();

            return GfxDeviceManager.Current.CreateIndexBuffer(desc);
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
